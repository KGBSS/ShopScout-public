using ShopScout.SharedLib.Models;

public interface ICategoryService
{
    Task<List<ProductCategory>> GetAll();
    Task<List<ProductCategory>> GetAllBottomLayer();
    Task<ProductCategory> GetById(int id);
}