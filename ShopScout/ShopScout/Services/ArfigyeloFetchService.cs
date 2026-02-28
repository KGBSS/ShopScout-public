using Microsoft.EntityFrameworkCore;
using ShopScout.Data;
using ShopScout.SharedLib.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopScout.Services;

public class ArfigyeloFetchService
{
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<ArfigyeloFetchService> _logger;

    public ArfigyeloFetchService(HttpClient httpClient,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<ArfigyeloFetchService> logger)
    {
        _httpClient = httpClient;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task FetchAsync()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var categories = _context.ProductCategories.Include(x => x.SubCategories)
                                                   .Where(x => x.Verified && x.SubCategories.Count == 0)
                                                   .Select(x => x.Name)
                                                   .ToList();
        var categoriesJson = await _httpClient.GetStringAsync($"https://arfigyelo.gvh.hu/api/categories");

        var leafCategories = MapCategoriesToIds(categories, categoriesJson);

        var ids = new ConcurrentBag<string>();
        await Stopper(GetIds);

        DateTime dt = DateTime.Now;
        await UpdateProductPricesAsync(ids);
        // await PopulateProductPerStoresFastAsync(ids);
        Console.WriteLine((DateTime.Now - dt).TotalSeconds);

        async Task GetIds()
        {
            var semaphore = new SemaphoreSlim(16);

            var idtasks = leafCategories.Select(async category =>
            {
                await semaphore.WaitAsync();
                try
                {
                    int offset = 0;
                    while (true)
                    {
                        var data = await _httpClient.GetStringAsync(
                            $"https://arfigyelo.gvh.hu/api/products-by-category/{category.Key}?limit=48&offset={offset}"
                        );
                        offset += 48;

                        var gids = GetIdsFromJsonString(data);
                        if (gids.Count == 0) break;
                        foreach (var id in gids)
                            ids.Add(id);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(idtasks);
        }
    }

    /// <summary>
    /// Maps the specified category names to their corresponding IDs by searching a hierarchical category structure
    /// represented in JSON.
    /// </summary>
    /// <param name="requestedNames">A list of category names to search for. Name comparisons are case-insensitive.</param>
    /// <param name="json">A JSON string representing the root of the category hierarchy. The JSON must match the expected structure for
    /// deserialization.</param>
    /// <returns>A dictionary mapping category IDs to their names for each requested name found in the JSON hierarchy. The
    /// dictionary is empty if none of the requested names are found.</returns>
    public static Dictionary<int, string> MapCategoriesToIds(List<string> requestedNames, string json)
    {
        // 1. Deserialize the JSON
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var root = JsonSerializer.Deserialize<CategoryRoot>(json, options);

        var result = new Dictionary<int, string>();

        // 2. Use a HashSet for faster lookups of the input list
        var searchSet = new HashSet<string>(requestedNames, StringComparer.OrdinalIgnoreCase);

        // 3. Define a recursive local function to traverse the tree
        void Traverse(List<CategoryNode> nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                // If this node's name is in our search list, add it to the dictionary
                if (searchSet.Contains(node.name))
                {
                    // We use TryAdd to avoid errors if the same ID appears twice in JSON
                    result.TryAdd(node.id, node.name);
                }

                // Recurse into children
                if (node.categoryNodes != null && node.categoryNodes.Count > 0)
                {
                    Traverse(node.categoryNodes);
                }
            }
        }

        // 4. Start traversal
        if (root?.categories != null)
        {
            Traverse(root.categories);
        }

        return result;
    }

    public async Task UpdateProductPricesAsync(ConcurrentBag<string> productIds)
    {
        // Configuration
        int batchSize = 50;
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 }; // Adjust based on CPU/DB limits

        // Performance tracking
        int totalProcessed = 0;
        int totalChanged = 0; // Counter for changes
        int totalCount = productIds.Count;
        _logger.LogInformation("Starting update for {totalCount} products...", totalCount);

        // 1. Chunk the data (processing 50 products per thread reduces DB roundtrips)
        var batches = productIds.Chunk(batchSize);

        // 2. Parallel Processing
        await Parallel.ForEachAsync(batches, parallelOptions, async (batchIds, ct) =>
        {
            try
            {
                int batchChanges = await ProcessBatchAsync(batchIds, ct);

                // Update counters
                if (batchChanges > 0)
                {
                    Interlocked.Add(ref totalChanged, batchChanges);
                }
                
                int current = Interlocked.Add(ref totalProcessed, batchIds.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch");
            }
        });

        _logger.LogInformation("Update complete. Changed {totalChanged} prices.", totalChanged);
    }

    private async Task<int> ProcessBatchAsync(string[] batchIds, CancellationToken ct)
    {
        // A. Fetch from API
        var apiDataList = await FetchApiDataForBatchAsync(batchIds, ct);
        if (!apiDataList.Any()) return 0;

        // Create lookup for fast matching: ProductID -> API Data
        var apiDataDict = apiDataList.ToDictionary(x => x.Id, x => x);
        var productCodes = apiDataDict.Keys.ToList();

        // B. Fetch from Database
        // CRITICAL: Create a FRESH DbContext for this specific thread/batch
        using var localContext = await _contextFactory.CreateDbContextAsync(ct);
        var dbEntities = await FetchDbEntitiesAsync(localContext, productCodes, ct);

        // C. Apply Logic (Pure function)
        int changesCount = UpdateEntitiesFromApiData(dbEntities, apiDataDict);

        // D. Save Changes
        if (changesCount > 0)
        {
            await localContext.SaveChangesAsync(ct);
        }

        return changesCount;
    }

    /// <summary>
    /// Fetches product data from the external API for a batch of IDs in parallel.
    /// </summary>
    public async Task<List<ApiProductResponse>> FetchApiDataForBatchAsync(string[] batchIds, CancellationToken ct)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Launch all HTTP requests for this batch simultaneously
        var tasks = batchIds.Select(async id =>
        {
            try
            {
                // _httpClient is thread-safe and should be reused
                var url = $"https://arfigyelo.gvh.hu/api/product/{id}";
                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<ApiProductResponse>(content, jsonOptions);
            }
            catch
            {
                // Log specific HTTP errors here if needed
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(x => x?.ChainStores != null).ToList()!;
    }

    /// <summary>
    /// Retrieves relevant ProductPerStore entities from the database for the given product codes.
    /// </summary>
    public async Task<List<ProductPerStore>> FetchDbEntitiesAsync(ApplicationDbContext context, ICollection<string> productCodes, CancellationToken ct)
    {
        return await context.Set<ProductPerStore>()
            .Include(pps => pps.Product)
            .Include(pps => pps.Store)
            .ThenInclude(s => s.StoreBrand)
            .Where(pps => productCodes.Contains(pps.Product.Code))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Matches API data to database entities and updates prices.
    /// Pure logic with no side effects or external dependencies.
    /// </summary>
    /// <returns>Number of modified entities.</returns>
    public static int UpdateEntitiesFromApiData(List<ProductPerStore> dbEntities, Dictionary<string, ApiProductResponse> apiDataDict)
    {
        int changesCount = 0;

        foreach (var entity in dbEntities)
        {
            var prodCode = entity.Product.Code;
            var storeBrandName = entity.Store.StoreBrand?.Name;

            // Match Product ID
            if (storeBrandName != null && apiDataDict.TryGetValue(prodCode, out var apiData))
            {
                // Match Store Name (e.g., "Tesco")
                var chainStore = apiData.ChainStores
                    .FirstOrDefault(cs => cs.Name.Equals(storeBrandName, StringComparison.OrdinalIgnoreCase));

                if (chainStore != null)
                {
                    if (ApplyPriceUpdates(entity, chainStore))
                    {
                        changesCount++;
                    }
                }
            }
        }

        return changesCount;
    }

    private static bool ApplyPriceUpdates(ProductPerStore entity, ApiChainStore chainStore)
    {
        var normalPriceObj = chainStore.Prices?.FirstOrDefault(p => p.Type == "NORMAL");
        var discountPriceObj = chainStore.Prices?.FirstOrDefault(p => p.Type == "DISCOUNTED");

        int? newNormal = normalPriceObj != null ? (int)normalPriceObj.Amount : null;
        int? newDiscount = discountPriceObj != null ? (int)discountPriceObj.Amount : null;

        bool changed = false;

        // Apply updates only if values changed
        if (newNormal.HasValue && entity.Price != newNormal.Value)
        {
            entity.Price = newNormal.Value;
            changed = true;
        }

        if (newDiscount.HasValue && entity.DiscountedPrice != newDiscount.Value)
        {
            entity.DiscountedPrice = newDiscount.Value;
            changed = true;
        }

        return changed;
    }

    //public async Task PopulateProductPerStoresFastAsync(ConcurrentBag<string> productCodes)
    //{
    //    Console.WriteLine("--- Starting Brand-Wide Population Process ---");

    //    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    //    // Limits: Process 50 products per batch, max 5 batches at a time
    //    var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
    //    int batchSize = 50;

    //    // --- COUNTER SETUP ---
    //    int totalProcessed = 0;
    //    // Distinct count is important so the progress bar ends exactly at 100%
    //    int totalCount = productCodes.Distinct().Count();

    //    // --- STEP 1: Pre-load Lookups ---
    //    using (var initialContext = await _contextFactory.CreateDbContextAsync())
    //    {
    //        Console.WriteLine("Pre-loading brand and product maps...");

    //        // Map: Brand Name -> List of Store IDs (e.g. "Tesco" -> [1, 2, 3...])
    //        var brandToStoreIdsMap = await initialContext.StoreBrands
    //            .Include(sb => sb.Stores)
    //            .ToDictionaryAsync(
    //                sb => sb.Name,
    //                sb => sb.Stores.Select(s => s.Id).ToList(),
    //                StringComparer.OrdinalIgnoreCase
    //            );

    //        // Map: Product Code -> Product ID
    //        var distinctCodes = productCodes.Distinct().ToList();
    //        var productCodeMap = await initialContext.Set<Product>()
    //            .Where(p => distinctCodes.Contains(p.Code))
    //            .Select(p => new { p.Code, p.Id })
    //            .ToDictionaryAsync(p => p.Code, p => p.Id);

    //        Console.WriteLine($"Maps loaded. Starting processing for {totalCount} products...");

    //        // --- STEP 2: Process in Batches ---
    //        var batches = distinctCodes.Chunk(batchSize);

    //        await Parallel.ForEachAsync(batches, parallelOptions, async (batchCodes, ct) =>
    //        {
    //            var newEntries = new List<ProductPerStore>();

    //            // A. Fetch API Data for Batch
    //            var tasks = batchCodes.Select(async code =>
    //            {
    //                if (!productCodeMap.TryGetValue(code, out int dbProductId)) return;

    //                try
    //                {
    //                    var response = await _httpClient.GetAsync($"https://arfigyelo.gvh.hu/api/product/{code}", ct);
    //                    if (!response.IsSuccessStatusCode) return;

    //                    var content = await response.Content.ReadAsStringAsync(ct);
    //                    var apiData = JsonSerializer.Deserialize<ApiProductResponse>(content, jsonOptions);

    //                    if (apiData?.ChainStores == null) return;

    //                    foreach (var chain in apiData.ChainStores)
    //                    {
    //                        // Get Prices
    //                        var normalObj = chain.Prices?.FirstOrDefault(p => p.Type == "NORMAL");
    //                        var discObj = chain.Prices?.FirstOrDefault(p => p.Type == "DISCOUNTED");

    //                        int? price = normalObj != null ? (int)normalObj.Amount : null;
    //                        int? discPrice = discObj != null ? (int)discObj.Amount : null;

    //                        if (price == null && discPrice == null) continue;

    //                        // Match Brand Name -> All Stores
    //                        if (brandToStoreIdsMap.TryGetValue(chain.Name, out var storeIds))
    //                        {
    //                            foreach (var storeId in storeIds)
    //                            {
    //                                lock (newEntries)
    //                                {
    //                                    newEntries.Add(new ProductPerStore
    //                                    {
    //                                        ProductId = dbProductId,
    //                                        StoreId = storeId,
    //                                        Price = price,
    //                                        DiscountedPrice = discPrice,
    //                                        Verified = false
    //                                    });
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //                catch { /* Ignore errors for individual items */ }
    //            });

    //            await Task.WhenAll(tasks);

    //            // B. Save to Database
    //            if (newEntries.Any())
    //            {
    //                using var saveContext = await _contextFactory.CreateDbContextAsync(ct);
    //                await saveContext.Set<ProductPerStore>().AddRangeAsync(newEntries, ct);
    //                await saveContext.SaveChangesAsync(ct);
    //            }

    //            // --- C. UPDATE COUNTER ---
    //            // Atomically add the batch size to the total
    //            int current = Interlocked.Add(ref totalProcessed, batchCodes.Length);

    //            // Calculate percentage
    //            double percent = (double)current / totalCount * 100;

    //            // Log progress
    //            Console.WriteLine($"[Progress] {current}/{totalCount} ({percent:F1}%)");
    //        });
    //    }

    //    Console.WriteLine("--- Population Complete ---");
    //}

    public class CategoryRoot
    {
        public List<CategoryNode> categories { get; set; }
    }

    public class CategoryNode
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<CategoryNode> categoryNodes { get; set; }
    }

    public class ApiProductResponse
    {
        public string Id { get; set; }
        public List<ApiChainStore> ChainStores { get; set; }
    }

    public class ApiChainStore
    {
        public string Name { get; set; }
        public List<ApiPrice> Prices { get; set; }
    }

    public class ApiPrice
    {
        public string Type { get; set; } // "NORMAL" or "DISCOUNTED"
        public double Amount { get; set; }
    }

    async Task Stopper(Func<Task> func)
    {
        DateTime start = DateTime.Now;
        await func();
        Console.WriteLine($"{func.Method.Name}: {(DateTime.Now - start).TotalSeconds}s");
    }

    // Helper class for SQL query result
    private class ProductCategoryRelation
    {
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
    }

    public static List<string> GetIdsFromJsonString(string jsonString)
    {
        var ids = new List<string>();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName &&
                reader.CurrentDepth == 3 &&
                reader.GetString() == "id")
            {
                reader.Read(); // Move to the value
                ids.Add(reader.GetString() ?? reader.GetInt32().ToString());
            }
        }

        return ids;
    }
}