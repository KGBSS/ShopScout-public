﻿using ShopScout.SharedLib.Models;

namespace ShopScout.SharedLib.Services
{
    public interface IProductService
    {
        Task<Product?> GetProductAsync(string barcode);
        Task<List<Product>> GetAllProductsAsync(int page);
        Task<List<Product>> GetProductsSearchAsync(string search_term, int page);
        Task<List<Product>> GetProductsFilteredAsync(string? search_term, ProductFilterParams filters, int page);
        Task<Product?> GetProductByIdAsync(string id);
        Task AttachRestAsync(Product product);
    }
}