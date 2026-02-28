using ShopScout.SharedLib.Models;

namespace ShopScout.SharedLib.Services;

public interface IAdminService
{
    Task<T> MergeEntities<T>(T entity1, T entity2, Dictionary<string, int> selections) where T : class;

    // Product methods
    Task<Product?> GetProductByIdAsync(int id);
    Task SaveProductAsync(Product product);

    // Generic lookup method
    Task<List<T>> GetAllAsync<T>() where T : class;

    // Create new entity
    Task<T> CreateNewAsync<T>(T entity) where T : class;
}