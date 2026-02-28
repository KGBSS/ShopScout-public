using Microsoft.EntityFrameworkCore.Internal;
using ShopScout.SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services;

public class ClientProductService : IProductService
{
    private readonly HttpClient _httpClient;
    public ClientProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Product>> GetAllProductsAsync(int page)
    {
        var result = await _httpClient.GetFromJsonAsync<List<Product>>($"/api/product/{page}");
        return result;
    }

    public async Task<Product?> GetProductAsync(string barcode)
    {
        var result = await _httpClient.GetFromJsonAsync<Product>($"/api/product/{barcode}");
        return result;
    }

    public async Task<List<Product>> GetProductsSearchAsync(string search_term, int page)
    {
        var result = await _httpClient.GetFromJsonAsync<List<Product>>($"/api/product/search/{search_term}/page/{page}");
        return result;
    }

    public async Task<List<Product>> GetProductsFilteredAsync(string? search_term, ProductFilterParams filters, int page)
    {
        var url = string.IsNullOrWhiteSpace(search_term)
            ? $"/api/product/filter/page/{page}"
            : $"/api/product/filter/{Uri.EscapeDataString(search_term)}/page/{page}";
        var result = await _httpClient.PostAsJsonAsync(url, filters);
        return await result.Content.ReadFromJsonAsync<List<Product>>() ?? new();
    }

    public Task AttachRestAsync(Product product)
    {
        throw new NotImplementedException();
    }
}