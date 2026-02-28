using Microsoft.EntityFrameworkCore;
using ShopScout.Data;
using ShopScout.Data.Migrations;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static ShopScout.Client.Pages.ProductPage;

namespace ShopScout.Services;

public class StoreService : IStoreService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly LocationService _locationService;
    private readonly HttpClient _httpClient;
    bool exited = false;
    public StoreService(IDbContextFactory<ApplicationDbContext> contextFactory,
                        HttpClient httpClient,
                        LocationService locationService)
    {
        _contextFactory = contextFactory;
        _httpClient = httpClient;
        _locationService = locationService;
    }

    public async Task AdminDeleteStoreAsync(Store store)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AdminUpdateStoreAsync(Store store)
    {
        try
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            _context.Stores.Update(store);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Store?> AdminGetStoreAsync(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Stores
            .Include(s => s.StoreBrand)
            .Include(s => s.StoreAttributes)
                .ThenInclude(sa => sa.StoreAttribute)
            .Include(s => s.ProductPerStore)
                .ThenInclude(pps => pps.Product)
            .Include(s => s.FavoritedBy)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Store?> GetStoreAsync(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.Stores
            .Include(s => s.StoreBrand)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AttachProducts(Store store)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        _context.Attach(store);
        await _context.Entry(store)
                      .Collection(s => s.ProductPerStore)
                      .Query()
                      .Include(pps => pps.Product)
                        .ThenInclude(p => p.ProductImages)
                      .Include(pps => pps.Product)
                        .ThenInclude(p => p.Categories)
                      .LoadAsync();

        await _context.Entry(store)
                      .Collection(s => s.StoreAttributes)
                      .Query()
                      .Include(sa => sa.StoreAttribute)
                      .LoadAsync();
    }

    public async Task MergeOSM()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        var storesString = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "shops.json"));

        var existingStores = await _context.Stores
            .Include(s => s.StoreAttributes)
            .Include(s => s.StoreBrand)
            .ToListAsync();

        var existingBrands = await _context.StoreBrands.ToListAsync();
        var existingAttributes = await _context.StoreAttributes.ToListAsync();

        var osmResponse = JsonSerializer.Deserialize<OsmResponse>(storesString);
        int counter1 = 0;
        foreach (var element in osmResponse.Elements)
        {
            var store = await DeserializeToStore(element, existingStores, existingBrands, existingAttributes);
            Console.Clear();
            Console.WriteLine($"{counter1++}.");
            // If it's a new store (not an existing one), add it to the context
            if (store.Id == 0)
            {
                _context.Stores.Add(store);
                existingStores.Add(store); // Add to in-memory list to prevent duplicates in same batch
            }
        }

        await _context.SaveChangesAsync();
        //foreach (var store in stores)
        //{
        //    var o = store.OpeningHours;
        //    if (o.StartsWith("Nyitvatartás:") && o.Contains("#Nyitvatartás - Vasárnap:") && o.Length < 70)
        //    {
        //        store.OpeningHours = $"{o[15..25]}6{o[52..]}";
        //    }
        //}
        //await _context.SaveChangesAsync();


        //var c = CalculateDistance(47.6587993, 19.2699863, 47.6587159, 19.2696037)*1000;
    }

    public async Task<Store> DeserializeToStore(OsmNodeResponse osmData, List<Store> existingStores, List<StoreBrand> existingBrands, List<StoreAttribute> existingAttributes)
    {
        var address = BuildAddress(osmData.Tags);

        string brandName = null;
        if (osmData.Tags != null)
        {
            osmData.Tags.TryGetValue("name", out var name);
            osmData.Tags.TryGetValue("brand", out var brand);
            brandName = !string.IsNullOrEmpty(name) ? name : brand;
        }
        bool found = false;
        Store existingStore = null;
        Console.WriteLine($"Existing: {brandName}: {address}");

        if (address == null)
        {
            found = true;
        }
        else
            foreach (var s in existingStores)
            {
                try
                {
                    if(s.StoreBrand == null || string.IsNullOrEmpty(s.StoreBrand.Name) || string.IsNullOrEmpty(s.Address) || string.IsNullOrEmpty(brandName))
                        continue;
                    if (string.Equals(s.Address.Replace(" ", "").Replace(".", ""), address.Replace(" ", "").Replace(".", ""), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(s.StoreBrand.Name.Replace(" ", "").Replace(".", ""), brandName.Replace(" ", "").Replace(".", ""), StringComparison.OrdinalIgnoreCase))
                    {
                        existingStore = s;
                        found = true;
                        break;
                    }
                }
                catch { }
            }

        if (!exited && !found)
        {
            foreach (var store1 in existingStores)
            {
                var dist = _locationService.CalculateDistance(
                    osmData.Type == "node" ? osmData.Lat.Value : osmData.Center.Lat,
                    osmData.Type == "node" ? osmData.Lon.Value : osmData.Center.Lon,
                    double.Parse(store1.Latitude),
                    double.Parse(store1.Longitude)
                    ) * 1000;
                if (dist < 50) // within 50 meters
                {
                    if(store1.StoreBrand != null)
                    {
                        Console.WriteLine($"Is this the same: {store1.StoreBrand.Name}: {store1.Address}?");
                    }
                    else
                    {
                        Console.WriteLine($"Is this the same: nobrand: {store1.Address}?");
                    }

                        var inp = Console.ReadKey();
                    if (inp.Key == ConsoleKey.Y)
                    {
                        existingStore = store1;
                        Console.WriteLine("Confirmed existing store.");
                        break;
                    }
                    else if (inp.Key == ConsoleKey.Q)
                    {
                        exited = true;
                        break;
                    }
                }
            }
        }

        if (existingStore != null)
        {
            // Update only if OsmId is empty/different
            if (existingStore.OsmId == 0 || existingStore.OsmId != osmData.Id)
                existingStore.OsmId = osmData.Id;

            // Update opening hours only if empty
            if (string.IsNullOrEmpty(existingStore.OpeningHours))
            {
                string openingHours = null;
                osmData.Tags?.TryGetValue("opening_hours", out openingHours);
                existingStore.OpeningHours = ParseOpeningHours(openingHours);
            }

            // Add new attributes
            await ProcessAttributes(existingStore, osmData.Tags, existingAttributes);

            return existingStore;
        }

        // Create new store
        var store = new Store
        {
            OsmId = osmData.Id,
            Latitude = osmData.Type == "node"
                ? osmData.Lat?.ToString()
                : osmData.Center?.Lat.ToString(),
            Longitude = osmData.Type == "node"
                ? osmData.Lon?.ToString()
                : osmData.Center?.Lon.ToString(),
            Address = address
        };

        // Parse opening hours
        if (osmData.Tags != null)
        {
            osmData.Tags.TryGetValue("opening_hours", out var openingHours);
            store.OpeningHours = ParseOpeningHours(openingHours);
        }

        // Find or create StoreBrand
        if (!string.IsNullOrEmpty(brandName))
        {
            var existingBrand = existingBrands
                .FirstOrDefault(b => b.Name.ToLower() == brandName.ToLower());

            if (existingBrand != null)
            {
                store.StoreBrand = existingBrand;
            }
            else
            {
                var newBrand = new StoreBrand { Name = brandName };
                existingBrands.Add(newBrand); // Add to in-memory list
                store.StoreBrand = newBrand;
            }
        }

        // Process attributes
        await ProcessAttributes(store, osmData.Tags, existingAttributes);

        return store;
    }

    private string BuildAddress(Dictionary<string, string> tags)
    {
        if (tags == null)
            return null;

        tags.TryGetValue("addr:postcode", out var postcode);
        tags.TryGetValue("addr:city", out var city);
        tags.TryGetValue("addr:street", out var street);
        tags.TryGetValue("addr:housenumber", out var housenumber);

        if (string.IsNullOrEmpty(postcode) && string.IsNullOrEmpty(city))
            return null;

        var parts = new List<string>();

        // First part: postcode + city
        var cityPart = "";
        if (!string.IsNullOrEmpty(postcode))
            cityPart = postcode;
        if (!string.IsNullOrEmpty(city))
            cityPart += string.IsNullOrEmpty(cityPart) ? city : " " + city;

        if (!string.IsNullOrEmpty(cityPart))
            parts.Add(cityPart);

        // Second part: street + housenumber
        var streetPart = "";
        if (!string.IsNullOrEmpty(street))
            streetPart = street;
        if (!string.IsNullOrEmpty(housenumber))
            streetPart += string.IsNullOrEmpty(streetPart) ? housenumber : " " + housenumber;

        if (!string.IsNullOrEmpty(streetPart))
            streetPart += ".";

        if (!string.IsNullOrEmpty(streetPart))
            parts.Add(streetPart);

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    private string ParseOpeningHours(string openingHours)
    {
        if (string.IsNullOrEmpty(openingHours))
            return null;

        // Split by semicolon for different day ranges
        var segments = openingHours.Split(';', StringSplitOptions.TrimEntries);

        var result = new StringBuilder();
        string firstHours = null;

        foreach (var segment in segments)
        {
            // Parse "Mo-Sa 06:30-21:00" or "Su 07:00-19:00"
            var parts = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                continue;

            var dayRange = parts[0];
            var hours = parts[1];

            if (firstHours == null)
            {
                firstHours = hours;
                result.Append(hours);
            }
            else
            {
                // Only add if this is a different time range
                var days = ParseDayRange(dayRange);
                foreach (var day in days)
                {
                    result.Append((int)day).Append(hours);
                }
            }
        }

        return result.Length > 0 ? result.ToString() : null;
    }

    private List<Day> ParseDayRange(string dayRange)
    {
        var days = new List<Day>();

        if (dayRange.Contains('-'))
        {
            var rangeParts = dayRange.Split('-');
            if (rangeParts.Length == 2 &&
                Enum.TryParse<Day>(rangeParts[0], out Day start) &&
                Enum.TryParse<Day>(rangeParts[1], out Day end))
            {
                for (int i = (int)start; i <= (int)end; i++)
                {
                    days.Add((Day)i);
                }
            }
        }
        else if (Enum.TryParse<Day>(dayRange, out Day singleDay))
        {
            days.Add(singleDay);
        }

        return days;
    }

    private async Task ProcessAttributes(Store store, Dictionary<string, string> tags, List<StoreAttribute> existingAttributes)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        if (tags == null)
            return;

        foreach (var tag in tags)
        {
            if (string.IsNullOrEmpty(tag.Value))
                continue;

            StoreAttributeType? attributeType = null;
            string attributeName = null;

            if (tag.Key.StartsWith("payment:"))
            {
                attributeType = StoreAttributeType.Payment;
                attributeName = tag.Key["payment:".Length..];
            }
            else if (tag.Key.StartsWith("contact:"))
            {
                attributeType = StoreAttributeType.Contact;
                attributeName = tag.Key["contact:".Length..];
            }

            if (attributeType.HasValue && !string.IsNullOrEmpty(attributeName))
            {
                // Check if attribute already exists in in-memory list
                var existingAttribute = existingAttributes
                    .FirstOrDefault(a => a.Name == attributeName && a.Type == attributeType.Value);

                if (existingAttribute == null)
                {
                    var newStoreAttribute = new StoreAttribute
                    {
                        Name = attributeName,
                        Type = attributeType.Value
                    };
                    var storeStoreAttribute = new StoreStoreAttribute()
                    {
                        StoreAttribute = newStoreAttribute,
                        Value = tag.Value
                    };
                    store.StoreAttributes.Add(storeStoreAttribute);
                    existingAttributes.Add(newStoreAttribute);
                    _context.StoreAttributes.Add(newStoreAttribute);
                }

                // Check if this store already has this attribute
                if (!store.StoreAttributes.Any(a => a.StoreAttribute.Name == attributeName && a.StoreAttribute.Type == attributeType.Value))
                {
                    store.StoreAttributes.Add(new StoreStoreAttribute()
                    {
                        StoreAttribute = existingAttribute,
                        Value = tag.Value
                    });
                }
            }
        }
    }

    public async Task<List<Store>> SearchStore(string searchText)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            var list = await _context.Stores.Include(s => s.StoreBrand)
                                        .Where(s => s.Verified && s.StoreBrand != null && !string.IsNullOrEmpty(s.StoreBrand.Name)).Take(20).ToListAsync();
            return list;
        }

        var searchLower = searchText.ToLower();

        var searchResults = await _context.Stores
            .Include(s => s.StoreBrand)
            .Where(s =>
                  (s.Address != null && EF.Functions.Collate(s.Address, "Latin1_General_CI_AI").Contains(searchLower)) ||
                  (s.StoreBrand != null && EF.Functions.Collate(s.StoreBrand.Name, "Latin1_General_CI_AI").Contains(searchLower))
            )
            .Take(20)
            .ToListAsync();

        return searchResults;
    }
}
public enum Day
{
    Mo = 0,
    Tu = 1,
    We = 2,
    Th = 3,
    Fr = 4,
    Sa = 5,
    Su = 6
}

public class OsmResponse
{
    [JsonPropertyName("elements")]
    public List<OsmNodeResponse> Elements { get; set; }
}

public class OsmNodeResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lon")]
    public double? Lon { get; set; }

    [JsonPropertyName("center")]
    public OsmCenter? Center { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; }
}

public class OsmCenter
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}