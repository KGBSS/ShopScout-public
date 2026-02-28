using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ShopScout.Data;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;
using System.Reflection;

namespace ShopScout.Services;

public class AdminService : IAdminService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AdminService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    #region Product Methods

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Products
            .Include(p => p.Details)
            .Include(p => p.ProductIngredients)
                .ThenInclude(pi => pi.Ingredient)
            .Include(p => p.ProductImages)
            .Include(p => p.Allergens)
            .Include(p => p.Additives)
            .Include(p => p.Labels)
            .Include(p => p.Packaging)
                .ThenInclude(pp => pp.Material)
            .Include(p => p.Packaging)
                .ThenInclude(pp => pp.Part)
            .Include(p => p.Attributes)
            .Include(p => p.ProductPerStore)
                .ThenInclude(pps => pps.Store)
                    .ThenInclude(s => s.StoreBrand)
            .Include(p => p.Brands)
            .Include(p => p.Countries)
            .Include(p => p.Categories)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task SaveProductAsync(Product product)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Check if product exists
        var existingProduct = await context.Products
            .Include(p => p.Details)
            .Include(p => p.ProductIngredients)
            .Include(p => p.ProductImages)
            .Include(p => p.Allergens)
            .Include(p => p.Additives)
            .Include(p => p.Labels)
            .Include(p => p.Packaging)
            .Include(p => p.Attributes)
            .Include(p => p.Brands)
            .Include(p => p.Countries)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existingProduct == null)
        {
            // New product
            context.Products.Add(product);
        }
        else
        {
            // Update existing product

            // Update scalar properties
            existingProduct.Code = product.Code;
            existingProduct.FromArfigyelo = product.FromArfigyelo;
            existingProduct.ProductName = product.ProductName;
            existingProduct.Description = product.Description;

            // Update Details
            if (product.Details != null)
            {
                if (existingProduct.Details == null)
                {
                    existingProduct.Details = product.Details;
                    existingProduct.Details.ProductId = existingProduct.Id;
                }
                else
                {
                    UpdateProductDetails(existingProduct.Details, product.Details);
                }
            }
            else if (existingProduct.Details != null)
            {
                context.Remove(existingProduct.Details);
                existingProduct.Details = null;
            }

            // Update collections
            await UpdateManyToManyCollection(context, existingProduct.Allergens, product.Allergens);
            await UpdateManyToManyCollection(context, existingProduct.Additives, product.Additives);
            await UpdateManyToManyCollection(context, existingProduct.Labels, product.Labels);
            await UpdateManyToManyCollection(context, existingProduct.Attributes, product.Attributes);
            await UpdateManyToManyCollection(context, existingProduct.Brands, product.Brands);
            await UpdateManyToManyCollection(context, existingProduct.Countries, product.Countries);
            await UpdateManyToManyCollection(context, existingProduct.Categories, product.Categories);
            await UpdateManyToManyCollection(context, existingProduct.Packaging, product.Packaging);

            // Update ProductIngredients
            UpdateProductIngredients(context, existingProduct, product);

            // Update ProductImages
            UpdateProductImages(context, existingProduct, product);
        }

        await context.SaveChangesAsync();
    }

    private void UpdateProductDetails(ProductDetails existing, ProductDetails updated)
    {
        existing.Quantity = updated.Quantity;
        existing.IngredientsText = updated.IngredientsText;
        existing.NutriScore = updated.NutriScore;
        existing.NovaGroup = updated.NovaGroup;
        existing.EnergyKcal = updated.EnergyKcal;
        existing.Fat = updated.Fat;
        existing.SaturatedFat = updated.SaturatedFat;
        existing.Carbohydrates = updated.Carbohydrates;
        existing.Sugars = updated.Sugars;
        existing.Proteins = updated.Proteins;
        existing.Salt = updated.Salt;
        existing.ServingSize = updated.ServingSize;
        existing.FatLevel = updated.FatLevel;
        existing.SaturatedFatLevel = updated.SaturatedFatLevel;
        existing.SaltLevel = updated.SaltLevel;
        existing.SugarsLevel = updated.SugarsLevel;
    }

    private async Task UpdateManyToManyCollection<T>(ApplicationDbContext context, ICollection<T> existing, ICollection<T> updated) where T : class
    {
        // Get the key property name
        var entityType = context.Model.FindEntityType(typeof(T));
        var keyProperty = entityType.FindPrimaryKey().Properties.First();
        var keyName = keyProperty.Name;

        // Remove items not in updated list
        var toRemove = existing.Where(e => !updated.Any(u =>
            GetPropertyValue(e, keyName).Equals(GetPropertyValue(u, keyName)))).ToList();

        foreach (var item in toRemove)
        {
            existing.Remove(item);
        }

        // Add new items
        var existingIds = existing.Select(e => GetPropertyValue(e, keyName)).ToHashSet();
        var toAdd = updated.Where(u => !existingIds.Contains(GetPropertyValue(u, keyName))).ToList();

        foreach (var item in toAdd)
        {
            // Attach the item from database
            var trackedItem = await context.Set<T>().FindAsync(GetPropertyValue(item, keyName));
            if (trackedItem != null)
            {
                existing.Add(trackedItem);
            }
        }
    }

    private void UpdateProductIngredients(ApplicationDbContext context, Product existing, Product updated)
    {
        // Remove deleted ingredients
        var toRemove = existing.ProductIngredients
            .Where(e => !updated.ProductIngredients.Any(u => u.IngredientId == e.IngredientId))
            .ToList();

        foreach (var item in toRemove)
        {
            existing.ProductIngredients.Remove(item);
        }

        // Add or update ingredients
        foreach (var updatedIngredient in updated.ProductIngredients)
        {
            var existingIngredient = existing.ProductIngredients
                .FirstOrDefault(e => e.IngredientId == updatedIngredient.IngredientId);

            if (existingIngredient == null)
            {
                // Add new
                existing.ProductIngredients.Add(new ProductProductIngredient
                {
                    ProductId = existing.Id,
                    IngredientId = updatedIngredient.IngredientId,
                    PercentEstimate = updatedIngredient.PercentEstimate
                });
            }
            else
            {
                // Update existing
                existingIngredient.PercentEstimate = updatedIngredient.PercentEstimate;
            }
        }
    }

    private void UpdateProductImages(ApplicationDbContext context, Product existing, Product updated)
    {
        // Remove deleted images
        var toRemove = existing.ProductImages
            .Where(e => !updated.ProductImages.Any(u => u.Id == e.Id))
            .ToList();

        foreach (var item in toRemove)
        {
            context.Remove(item);
            existing.ProductImages.Remove(item);
        }

        // Add or update images
        foreach (var updatedImage in updated.ProductImages)
        {
            if (updatedImage.Id == 0)
            {
                // New image
                updatedImage.Product = existing;
                existing.ProductImages.Add(updatedImage);
            }
            else
            {
                // Update existing
                var existingImage = existing.ProductImages.FirstOrDefault(i => i.Id == updatedImage.Id);
                if (existingImage != null)
                {
                    existingImage.URL = updatedImage.URL;
                    existingImage.ImageType = updatedImage.ImageType;
                }
            }
        }
    }

    private void UpdateProductPerStore(ApplicationDbContext context, Product existing, Product updated)
    {
        // Remove deleted store associations
        var toRemove = existing.ProductPerStore
            .Where(e => !updated.ProductPerStore.Any(u => u.StoreId == e.StoreId))
            .ToList();

        foreach (var item in toRemove)
        {
            context.Remove(item);
            existing.ProductPerStore.Remove(item);
        }

        // Add or update store associations
        foreach (var updatedStore in updated.ProductPerStore)
        {
            var existingStore = existing.ProductPerStore
                .FirstOrDefault(e => e.StoreId == updatedStore.StoreId);

            if (existingStore == null)
            {
                // Add new
                existing.ProductPerStore.Add(new ProductPerStore
                {
                    ProductId = existing.Id,
                    StoreId = updatedStore.StoreId,
                    Price = updatedStore.Price,
                    DiscountedPrice = updatedStore.DiscountedPrice,
                    Verified = updatedStore.Verified
                });
            }
            else
            {
                // Update existing
                existingStore.Price = updatedStore.Price;
                existingStore.DiscountedPrice = updatedStore.DiscountedPrice;
                existingStore.Verified = updatedStore.Verified;
            }
        }
    }

    private object GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    }

    #endregion

    #region Generic Lookup Method

    public async Task<List<T>> GetAllAsync<T>() where T : class
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Set<T>().AsQueryable();

        // Special handling for entities that need includes
        if (typeof(T) == typeof(ProductPackaging))
        {
            query = query.Include("Material").Include("Part");
        }
        else if (typeof(T) == typeof(Store))
        {
            query = query.Include("StoreBrand");
        }

        // Try to order by Name if the property exists
        var nameProperty = typeof(T).GetProperty("Name");
        if (nameProperty != null)
        {
            query = query.OrderBy(e => EF.Property<string>(e, "Name"));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<T> CreateNewAsync<T>(T entity) where T : class
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var entry = await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();

        return entry.Entity;
    }

    #endregion

    #region Merge Entities (existing code)

    public async Task<T> MergeEntities<T>(T entity1, T entity2, Dictionary<string, int> selections) where T : class
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Get primary key property
        var entityType = context.Model.FindEntityType(typeof(T));
        var keyProperty = entityType.FindPrimaryKey().Properties.First();
        var keyName = keyProperty.PropertyInfo.Name;

        var id1 = typeof(T).GetProperty(keyName).GetValue(entity1);
        var id2 = typeof(T).GetProperty(keyName).GetValue(entity2);

        if (id1 == null || id2 == null)
            throw new InvalidOperationException("Entities must have valid IDs");

        // Load both entities with all navigation properties
        var dbSet = context.Set<T>();
        var query1 = dbSet.Where(e => EF.Property<object>(e, keyName).Equals(id1));
        var query2 = dbSet.Where(e => EF.Property<object>(e, keyName).Equals(id2));

        // Include all collection navigations
        foreach (var navigation in entityType.GetNavigations())
        {
            if (navigation.IsCollection)
            {
                var navName = navigation.Name;
                query1 = query1.Include(navName);
                query2 = query2.Include(navName);
            }
        }

        var dbEntity1 = await query1.FirstOrDefaultAsync();
        var dbEntity2 = await query2.FirstOrDefaultAsync();

        if (dbEntity1 == null || dbEntity2 == null)
            throw new InvalidOperationException($"Could not find entities with IDs {id1} and {id2}");

        // Update scalar properties
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.CanWrite && !IsNavigationProperty(prop) && prop.Name != keyName)
            {
                var selectedIndex = selections.GetValueOrDefault(prop.Name, 0);
                var sourceEntity = selectedIndex == 0 ? dbEntity1 : dbEntity2;
                var value = prop.GetValue(sourceEntity);
                prop.SetValue(dbEntity1, value);
            }
        }

        // Handle collection navigation properties
        foreach (var navigation in entityType.GetNavigations())
        {
            if (navigation.IsCollection)
            {
                var propName = navigation.Name;
                var selectedIndex = selections.GetValueOrDefault(propName, 0);

                if (selectedIndex == 1) // Use entity2's collection
                {
                    var prop = typeof(T).GetProperty(propName);
                    var collection1 = prop.GetValue(dbEntity1) as System.Collections.IList;
                    var collection2 = prop.GetValue(dbEntity2) as System.Collections.IEnumerable;

                    if (collection1 != null && collection2 != null)
                    {
                        // Get the item type
                        var itemType = collection1.GetType().GetGenericArguments()[0];

                        // Clear entity1's collection first (this deletes the old items)
                        collection1.Clear();

                        // Clone and add items from entity2's collection
                        foreach (var item in collection2.Cast<object>().ToList())
                        {
                            var clonedItem = CloneEntityForCollection(item, dbEntity1, entityType, context);
                            collection1.Add(clonedItem);
                        }
                    }
                }
            }
        }

        // Delete entity2
        context.Remove(dbEntity2);

        await context.SaveChangesAsync();
        return dbEntity1;
    }

    private object CloneEntityForCollection(object source, object parent, Microsoft.EntityFrameworkCore.Metadata.IEntityType parentEntityType, DbContext context)
    {
        var sourceType = source.GetType();
        var clone = Activator.CreateInstance(sourceType);

        var itemEntityType = context.Model.FindEntityType(sourceType);

        // Find which foreign key points to our parent
        var fkToParent = itemEntityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType == parentEntityType);

        var fkToParentPropertyNames = fkToParent?.Properties.Select(p => p.Name).ToHashSet() ?? new HashSet<string>();

        foreach (var prop in sourceType.GetProperties())
        {
            if (!prop.CanWrite)
                continue;

            var value = prop.GetValue(source);

            // Find navigation to parent
            var navToParent = itemEntityType.GetNavigations()
                .FirstOrDefault(n => n.ForeignKey == fkToParent && n.Name == prop.Name);

            if (navToParent != null)
            {
                // Set parent navigation
                prop.SetValue(clone, parent);
            }
            else if (fkToParentPropertyNames.Contains(prop.Name))
            {
                // Skip FK properties to parent (will be set by navigation)
                continue;
            }
            else
            {
                // Copy everything else (including other FKs and navigations)
                prop.SetValue(clone, value);
            }
        }

        return clone;
    }

    private bool IsNavigationProperty(PropertyInfo prop)
    {
        var type = prop.PropertyType;

        // Collection navigation
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return true;

        // Reference navigation (complex type)
        if (type.IsClass && type != typeof(string) && !type.IsValueType && type.Namespace != "System")
            return true;

        return false;
    }

    #endregion
}