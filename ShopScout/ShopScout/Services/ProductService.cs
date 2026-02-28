using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShopScout.Client.Components;
using ShopScout.Client.Pages;
using ShopScout.Data;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShopScout.Services;

public class ProductService : IProductService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackgroundTaskQueue _backgroundQueue;

    public ProductService(IDbContextFactory<ApplicationDbContext> contextFactory, HttpClient httpClient,
        IServiceProvider serviceProvider, IBackgroundTaskQueue backgroundQueue)
    {
        _contextFactory = contextFactory;
        _httpClient = httpClient;
        _backgroundQueue = backgroundQueue;
        _serviceProvider = serviceProvider;
    }

    public async Task<Product?> GetProductByIdAsync(string id) // only admin use
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        if (!int.TryParse(id, out var intid)) return null;

        return await _context.Products
                       .Include(p => p.ProductImages)
                       .Include(p => p.Details)
                       .Include(p => p.Brands)
                       .Include(p => p.Countries)
                       .FirstOrDefaultAsync(p => p.Id == intid);
    }

    public async Task<Product?> GetProductAsync(string barcode)
    {
        try
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            // First, check if product exists in database
            bool productExists = await _context.Products.AnyAsync(p => p.Code == barcode);

            if (productExists)
            {
                var existingProduct = await _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.Categories)
                    .Include(p => p.Details)
                    .Include(p => p.Brands)
                    .FirstOrDefaultAsync(p => p.Code == barcode);

                return existingProduct;
            }

            // If not found, fetch from OpenFoodFacts API
            var response = await _httpClient.GetFromJsonAsync<OpenFoodFactsResponse>(
                $"https://world.openfoodfacts.org/api/v0/product/{barcode.Trim()}.json");

            if (response?.Status == 1 && response.Product != null)
            {
                var product = ConvertProduct(response.Product);
                await QueueSaveAsync(response.Product);
                return product;
            }
        }
        catch { }

        return null;
    }

    private async Task QueueSaveAsync(OpenFoodFactsProduct openFoodFactsProduct)
    {
        await _backgroundQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                ProductDetails details;
                if (openFoodFactsProduct.Nutriments == null)
                {
                    details = new()
                    {
                        Quantity = openFoodFactsProduct.Quantity,
                        IngredientsText = openFoodFactsProduct.IngredientsText,
                        NutriScore = ParseNutriScore(openFoodFactsProduct.NutriScore),
                        NovaGroup = (byte)openFoodFactsProduct.NovaGroup,
                        ServingSize = openFoodFactsProduct.ServingSize
                    };
                }
                else
                {
                    details = new()
                    {
                        EnergyKcal = openFoodFactsProduct.Nutriments.EnergyKcal,
                        Fat = openFoodFactsProduct.Nutriments.Fat,
                        SaturatedFat = openFoodFactsProduct.Nutriments.SaturatedFat,
                        Carbohydrates = openFoodFactsProduct.Nutriments.Carbohydrates,
                        Sugars = openFoodFactsProduct.Nutriments.Sugars,
                        Proteins = openFoodFactsProduct.Nutriments.Proteins,
                        Salt = openFoodFactsProduct.Nutriments.Salt,
                        Quantity = openFoodFactsProduct.Quantity,
                        IngredientsText = openFoodFactsProduct.IngredientsText,
                        NutriScore = ParseNutriScore(openFoodFactsProduct.NutriScore),
                        NovaGroup = (byte)openFoodFactsProduct.NovaGroup,
                        ServingSize = openFoodFactsProduct.ServingSize
                    };
                }

                var product = new Product
                {
                    Code = openFoodFactsProduct.Code,
                    ProductName = openFoodFactsProduct.ProductName ?? "",
                    Description = openFoodFactsProduct.GenericName,
                    Details = details
                };

                context.Products.Add(product);

                if (!string.IsNullOrEmpty(openFoodFactsProduct.ImageUrl))
                {
                    product.ProductImages.Add(new ProductImage()
                    {
                        URL = openFoodFactsProduct.ImageUrl,
                        ImageType = ProductImageType.Primary
                    });
                }

                if (!string.IsNullOrEmpty(openFoodFactsProduct.ImageIngredientsUrl))
                {
                    product.ProductImages.Add(new ProductImage()
                    {
                        URL = openFoodFactsProduct.ImageIngredientsUrl,
                        ImageType = ProductImageType.Ingredients
                    });
                }

                if (!string.IsNullOrEmpty(openFoodFactsProduct.ImageNutritionUrl))
                {
                    product.ProductImages.Add(new ProductImage()
                    {
                        URL = openFoodFactsProduct.ImageNutritionUrl,
                        ImageType = ProductImageType.Nutrition
                    });
                }

                if (!string.IsNullOrEmpty(openFoodFactsProduct.ImagePackagingUrl))
                {
                    product.ProductImages.Add(new ProductImage()
                    {
                        URL = openFoodFactsProduct.ImagePackagingUrl,
                        ImageType = ProductImageType.Packaging
                    });
                }

                // Save Ingredients
                if (openFoodFactsProduct.Ingredients != null)
                {
                    var existingIngredients = await context.ProductIngredients.ToListAsync();

                    for (int i = 0; i < openFoodFactsProduct.Ingredients.Count; i++)
                    {
                        var ingredient = openFoodFactsProduct.Ingredients[i];
                        if (ingredient.HasSubIngredients?.Equals("yes", StringComparison.OrdinalIgnoreCase) != true)
                        {
                            var existing = existingIngredients.FirstOrDefault(x => x.Name.Trim().Equals(ingredient.Text, StringComparison.OrdinalIgnoreCase));
                            if (existing is null)
                            {
                                existing = new ProductIngredient()
                                {
                                    Name = ingredient.Text.Trim()
                                };
                                existingIngredients.Add(existing);
                            }
                            product.ProductIngredients.Add(new ProductProductIngredient()
                            {
                                Ingredient = existing,
                                PercentEstimate = Math.Round(ingredient.PercentEstimate ?? 0, 2)
                            });
                        }
                    }
                }

                // Save Packaging
                if (openFoodFactsProduct.Packaging != null)
                {
                    foreach (var packaging in openFoodFactsProduct.Packaging)
                    {
                        ProductPackaging productPackaging = new()
                        {
                            QuantityPerUnit = packaging.QuantityPerUnit,
                            Recyclable = packaging.Recycling?.Contains("recycle") == true // might be non-recyclable ---------------------------------------
                        };
                        AttachOrCreateNTo1<PackagingMaterial>(packaging.Material, context.PackagingMaterials, x => productPackaging.Material = x);
                        AttachOrCreateNTo1<PackagingPart>(packaging.Shape, context.PackagingParts, x => productPackaging.Part = x);
                        product.Packaging.Add(productPackaging);
                        context.ProductPackagings.Add(productPackaging);
                    }
                }

                if (openFoodFactsProduct.NutrientLevels != null)
                {
                    foreach (var level in openFoodFactsProduct.NutrientLevels)
                    {
                        var propInfo = typeof(ProductDetails).GetProperty($"{level.Key.Replace("-", "")}Level", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                        if (propInfo != null)
                        {
                            if (Enum.TryParse<NutrientLevel>(level.Value, out var enumValue))
                            {
                                propInfo.SetValue(details, enumValue);
                            }
                        }
                    }
                }

                AttachOrCreateNToN<ProductLabel>(openFoodFactsProduct.Labels, context.ProductLabels, product.Labels);
                AttachOrCreateNToN<ProductAllergen>(openFoodFactsProduct.Allergens, context.ProductAllergens, product.Allergens);
                AttachOrCreateNToN<ProductAdditive>(openFoodFactsProduct.Additives, context.ProductAdditives, product.Additives);
                AttachOrCreateNToN<ProductAttribute>(openFoodFactsProduct.IngredientsAnalysisTags, context.ProductAttributes, product.Attributes);
                AttachOrCreateNToN<ProductBrand>(openFoodFactsProduct.Brands, context.ProductBrands, product.Brands);
                AttachOrCreateNToN<ProductCategory>(openFoodFactsProduct.Categories, context.ProductCategories, product.Categories);
                AttachOrCreateNToN<ProductCountry>(openFoodFactsProduct.Countries, context.ProductCountries, product.Countries);

                void AttachOrCreateNTo1<T>(string item, DbSet<T> table, Action<T> setTarget) where T : class, INToNTable
                {
                    if (item == null) return;
                    var formattedItem = item.Split(":").Last().Replace("-", " ").Replace("_", " ");
                    var dbItem = table.FirstOrDefault(x => x.Name == formattedItem);

                    if (dbItem == null)
                    {
                        dbItem = (T)Activator.CreateInstance(typeof(T));
                        dbItem.Name = formattedItem;
                        table.Add(dbItem);
                    }

                    setTarget(dbItem);
                }

                void AttachOrCreateNToN<T>(List<string> collection, DbSet<T> table, ICollection<T> attachingCollection) where T : class, INToNTable
                {
                    if (collection == null) return;
                    foreach (var item in collection)
                    {
                        var formattedItem = item.Split(":").Last().Replace("-", " ").Replace("_", " ");
                        var dbItem = table.FirstOrDefault(x => x.Name == formattedItem);

                        if (dbItem == null)
                        {
                            dbItem = (T)Activator.CreateInstance(typeof(T));
                            dbItem.Name = formattedItem;
                            table.Add(dbItem);
                        }

                        attachingCollection.Add(dbItem);
                    }
                }

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }

    private Product ConvertProduct(OpenFoodFactsProduct openFoodFactsProduct)
    {
        ProductDetails details;
        if (openFoodFactsProduct.Nutriments == null)
        {
            details = new()
            {
                Quantity = openFoodFactsProduct.Quantity,
                IngredientsText = openFoodFactsProduct.IngredientsText,
                NutriScore = ParseNutriScore(openFoodFactsProduct.NutriScore),
                NovaGroup = (byte)openFoodFactsProduct.NovaGroup,
                ServingSize = openFoodFactsProduct.ServingSize
            };
        }
        else
        {
            details = new()
            {
                EnergyKcal = openFoodFactsProduct.Nutriments.EnergyKcal,
                Fat = openFoodFactsProduct.Nutriments.Fat,
                SaturatedFat = openFoodFactsProduct.Nutriments.SaturatedFat,
                Carbohydrates = openFoodFactsProduct.Nutriments.Carbohydrates,
                Sugars = openFoodFactsProduct.Nutriments.Sugars,
                Proteins = openFoodFactsProduct.Nutriments.Proteins,
                Salt = openFoodFactsProduct.Nutriments.Salt,
                Quantity = openFoodFactsProduct.Quantity,
                IngredientsText = openFoodFactsProduct.IngredientsText,
                NutriScore = ParseNutriScore(openFoodFactsProduct.NutriScore),
                NovaGroup = (byte)openFoodFactsProduct.NovaGroup,
                ServingSize = openFoodFactsProduct.ServingSize
            };
        }

        var product = new Product
        {
            Code = openFoodFactsProduct.Code,
            ProductName = openFoodFactsProduct.ProductName ?? "",
            Description = openFoodFactsProduct.GenericName,
            Details = details
        };

        if (!string.IsNullOrEmpty(openFoodFactsProduct.ImageUrl))
        {
            product.ProductImages.Add(new ProductImage()
            {
                URL = openFoodFactsProduct.ImageUrl,
                ImageType = ProductImageType.Primary
            });
        }

        if (!string.IsNullOrEmpty(openFoodFactsProduct.ImageIngredientsUrl))
        {
            product.ProductImages.Add(new ProductImage()
            {
                URL = openFoodFactsProduct.ImageIngredientsUrl,
                ImageType = ProductImageType.Ingredients
            });
        }

        if (!string.IsNullOrEmpty(openFoodFactsProduct.ImageNutritionUrl))
        {
            product.ProductImages.Add(new ProductImage()
            {
                URL = openFoodFactsProduct.ImageNutritionUrl,
                ImageType = ProductImageType.Nutrition
            });
        }

        if (!string.IsNullOrEmpty(openFoodFactsProduct.ImagePackagingUrl))
        {
            product.ProductImages.Add(new ProductImage()
            {
                URL = openFoodFactsProduct.ImagePackagingUrl,
                ImageType = ProductImageType.Packaging
            });
        }

        // Save Ingredients
        if (openFoodFactsProduct.Ingredients != null)
        {
            for (int i = 0; i < openFoodFactsProduct.Ingredients.Count; i++)
            {
                var ingredient = openFoodFactsProduct.Ingredients[i];
                if (ingredient.HasSubIngredients?.Equals("yes", StringComparison.OrdinalIgnoreCase) != true)
                {
                    
                    var existing = new ProductIngredient()
                    {
                        Name = ingredient.Text.Trim()
                    };
                    
                    product.ProductIngredients.Add(new ProductProductIngredient()
                    {
                        Ingredient = existing,
                        PercentEstimate = Math.Round(ingredient.PercentEstimate ?? 0, 2)
                    });
                }
            }
        }

        // Save Packaging
        if (openFoodFactsProduct.Packaging != null)
        {
            foreach (var packaging in openFoodFactsProduct.Packaging)
            {
                ProductPackaging productPackaging = new()
                {
                    QuantityPerUnit = packaging.QuantityPerUnit,
                    Recyclable = packaging.Recycling?.Contains("recycle") == true // might be non-recyclable ------------------------------
                };
                AttachToProperty<PackagingMaterial>(packaging.Material, x => productPackaging.Material = x);
                AttachToProperty<PackagingPart>(packaging.Shape, x => productPackaging.Part = x);

                if (productPackaging.Part != null || productPackaging.Material != null)
                    product.Packaging.Add(productPackaging);
            }
        }

        // Save Nutrient Levels ------------------------------------------------------- URGENTLY TEST ts
        if (openFoodFactsProduct.NutrientLevels != null)
        {
            foreach (var level in openFoodFactsProduct.NutrientLevels)
            {
                var propInfo = typeof(ProductDetails).GetProperty($"{level.Key.Replace("-", "")}Level", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propInfo != null)
                {
                    if (Enum.TryParse<NutrientLevel>(level.Value, out var enumValue))
                    {
                        propInfo.SetValue(details, enumValue);
                    }
                }
            }
        }

        AttachToCollection(openFoodFactsProduct.Labels, product.Labels);
        AttachToCollection(openFoodFactsProduct.Allergens, product.Allergens);
        AttachToCollection(openFoodFactsProduct.Additives, product.Additives);
        AttachToCollection(openFoodFactsProduct.IngredientsAnalysisTags, product.Attributes);
        AttachToCollection(openFoodFactsProduct.Brands, product.Brands);
        AttachToCollection(openFoodFactsProduct.Categories, product.Categories);
        AttachToCollection(openFoodFactsProduct.Countries, product.Countries);

        return product;
    }

    /// <summary>
    /// Creates and adds new instances of type T to the specified collection, using formatted values from the provided
    /// list of strings.
    /// </summary>
    /// <typeparam name="T">The type of objects to create and add to the collection. Must be a class that implements INToNTable.</typeparam>
    /// <param name="collection">A list of strings whose values are used to generate and format the Name property of each new T instance. Each
    /// string should be in the expected format; null values are ignored.</param>
    /// <param name="attachingCollection">The collection to which the new instances of type T are added. Must not be null.</param>
    internal static void AttachToCollection<T>(List<string> collection, ICollection<T> attachingCollection) where T : class, INToNTable
    {
        if (collection == null) return;
        foreach (var item in collection)
        {
            if (item == null) return;
            var formattedItem = item.Split(":").Last().Replace("-", " ").Replace("_", " ");

            var dbItem = (T)Activator.CreateInstance(typeof(T));
            dbItem.Name = formattedItem;

            attachingCollection.Add(dbItem);
        }
    }

    /// <summary>
    /// Creates an instance of the specified table type from the provided item name and assigns it to the target
    /// property using the specified setter.
    /// </summary>
    /// <typeparam name="T">The type of table to instantiate. Must implement the INToNTable interface and have a parameterless constructor.</typeparam>
    /// <param name="item">The string representation of the item to attach. If null, the method does nothing.</param>
    /// <param name="setTarget">An action delegate that receives the newly created table instance. Used to assign the instance to the target
    /// property.</param>
    internal static void AttachToProperty<T>(string item, Action<T> setTarget) where T : class, INToNTable
    {
        if (item == null) return;
        var formattedItem = item.Split(":").Last().Replace("-", " ").Replace("_", " ");

        var dbItem = (T)Activator.CreateInstance(typeof(T));
        dbItem.Name = formattedItem;

        setTarget(dbItem);
    }

    public async Task<List<Product>> GetAllProductsAsync(int page)
    {
        try
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var data = await _context.Products.Include(p => p.Brands)
                                              .Include(p => p.ProductImages)
                                              .Include(p => p.Categories)
                                              .Where(p => p.FromArfigyelo && p.ProductImages.Count > 1) // default view displays only products that don't miss data
                                              .Skip(20 * (page - 1)).Take(20).ToListAsync();
            return data;
        }
        catch { }
        return null;
    }

    public async Task<List<Product>> GetProductsSearchAsync(string search_term, int page)
    {
        search_term = search_term.Trim().ToLower();
        try
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var products = await _context.Products
            .Include(p => p.Brands)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductPerStore)
            .Where(p =>
                EF.Functions.Like(p.ProductName.ToLower(), $"%{search_term}%") ||
                EF.Functions.Like(p.Description.ToLower(), $"%{search_term}%") ||
                EF.Functions.Like(p.Brands.FirstOrDefault().Name ?? "Unknown brand".ToLower(), $"%{search_term}%")
            )
            .OrderByDescending(p =>
                //!string.IsNullOrEmpty(p.ImageUrl) ? 7 :
                //(p.ProductName.ToLower().StartsWith(search_term) ? 6 :
                //p.ProductName.ToLower().Contains(search_term) ? 5 :
                //p.GenericName.ToLower().StartsWith(search_term) ? 4 :
                //p.GenericName.ToLower().Contains(search_term) ? 3 :
                //p.Brands.FirstOrDefault().Name.ToLower().StartsWith(search_term) ? 2 :
                //p.Brands.FirstOrDefault().Name.ToLower().Contains(search_term) ? 1 : 0)

                !string.IsNullOrEmpty(p.ProductImages.FirstOrDefault(x => x.ImageType == ProductImageType.Primary).URL) ? 10 :
                p.ProductName.ToLower().StartsWith(search_term) ? 9 :
                EF.Functions.Like(" " + p.ProductName.ToLower() + " ", $"% {search_term} %") ? 8 :
                p.ProductName.ToLower().Contains(search_term) ? 7 :
                p.Description.ToLower().StartsWith(search_term) ? 6 :
                EF.Functions.Like(" " + p.Description.ToLower() + " ", $"% {search_term} %") ? 5 :
                p.Description.ToLower().Contains(search_term) ? 4 :
                p.Brands.FirstOrDefault().Name.ToLower().StartsWith(search_term) ? 3 :
                EF.Functions.Like(" " + p.Brands.FirstOrDefault().Name.ToLower() + " ", $"% {search_term} %") ? 2 :
                p.Brands.FirstOrDefault().Name.ToLower().Contains(search_term) ? 1 : 0
            )
            .ThenBy(p => p.ProductName)
            .Skip(20 * (page - 1))
            .Take(20) // optional: limit results
            .AsNoTracking()
            .ToListAsync();
            return products;
        }
        catch { }
        return null;
    }

    public async Task<List<Product>> GetProductsFilteredAsync(string? search_term, ProductFilterParams filters, int page)
    {
        try
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var q = _context.Products
                .Include(p => p.Brands)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Only include navigation properties needed by active filters
            if (filters.AllergenIds.Count > 0)
                q = q.Include(p => p.Allergens);
            if (filters.LabelIds.Count > 0)
                q = q.Include(p => p.Labels);
            if (filters.AttributeIds.Count > 0)
                q = q.Include(p => p.Attributes);
            if (filters.CountryIds.Count > 0)
                q = q.Include(p => p.Countries);
            if (filters.CategoryIds.Count > 0)
                q = q.Include(p => p.Categories);
            if (filters.StoreBrandIds.Count > 0)
                q = q.Include(p => p.ProductPerStore).ThenInclude(pps => pps.Store).ThenInclude(s => s.StoreBrand);

            if (!string.IsNullOrWhiteSpace(search_term))
            {
                var term = search_term.Trim().ToLower();
                q = q.Where(p =>
                    EF.Functions.Like(p.ProductName.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Description.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Brands.FirstOrDefault().Name ?? "", $"%{term}%"));
            }

            if (filters.FromArfigyelo.HasValue)
                q = q.Where(p => p.FromArfigyelo == filters.FromArfigyelo.Value);

            if (filters.AllergenIds.Count > 0)
                q = q.Where(p => p.Allergens.Any(a => filters.AllergenIds.Contains(a.Id)));

            if (filters.LabelIds.Count > 0)
                q = q.Where(p => p.Labels.Any(l => filters.LabelIds.Contains(l.Id)));

            if (filters.AttributeIds.Count > 0)
                q = q.Where(p => p.Attributes.Any(a => filters.AttributeIds.Contains(a.Id)));

            if (filters.BrandIds.Count > 0)
                q = q.Where(p => p.Brands.Any(b => filters.BrandIds.Contains(b.Id)));

            if (filters.CountryIds.Count > 0)
                q = q.Where(p => p.Countries.Any(c => filters.CountryIds.Contains(c.Id)));

            if (filters.CategoryIds.Count > 0)
                q = q.Where(p => p.Categories.Any(c => filters.CategoryIds.Contains(c.Id)));

            if (filters.StoreBrandIds.Count > 0)
                q = q.Where(p => p.ProductPerStore.Any(pps =>
                    pps.Store.StoreBrand != null && filters.StoreBrandIds.Contains(pps.Store.StoreBrand.Id)));

            return await q
                .OrderBy(p => p.ProductName)
                .Skip(20 * (page - 1)).Take(20)
                .AsNoTracking()
                .ToListAsync();
        }
        catch { }
        return new List<Product>();
    }

    public async Task AttachRestAsync(Product product)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        _context.Attach(product);

        var entry = _context.Entry(product);

        await entry
            .Collection(p => p.ProductPerStore)
            .Query()
            .Include(pps => pps.Store)
                .ThenInclude(s => s.StoreBrand)
            .LoadAsync();

        await entry
            .Collection(p => p.ProductIngredients)
            .Query()
            .Include(p => p.Ingredient)
            .OrderByDescending(i => i.PercentEstimate)
            .LoadAsync();

        await entry.Collection(p => p.Allergens).LoadAsync();
        await entry.Collection(p => p.Additives).LoadAsync();
        await entry.Collection(p => p.Labels).LoadAsync();
        await entry.Collection(p => p.Countries).LoadAsync();

        await entry
            .Collection(p => p.Packaging)
            .Query()
            .Include(p => p.Material)
            .Include(p => p.Part)
            .LoadAsync();

        await entry.Collection(p => p.Attributes).LoadAsync();
    }

    private NutriScore ParseNutriScore(string? score)
    {
        return Enum.TryParse<NutriScore>(score, true, out var nutriVal) ? nutriVal : NutriScore.Unknown;
    }
}