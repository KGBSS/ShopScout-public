using Microsoft.EntityFrameworkCore.Diagnostics;
using ShopScout.SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services
{
    public class ClientStoreLayoutService : IStoreLayoutService
    {
        private readonly HttpClient _httpClient;
        public ClientStoreLayoutService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<Store?> GetStoreAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Store>($"api/StoreLayout/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<Store> StoreWalls(LayoutDto layoutDto, int id)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/StoreLayout/{id}", layoutDto);
            response.EnsureSuccessStatusCode();

            var savedLayout = await response.Content.ReadFromJsonAsync<Store>();
            if (savedLayout == null)
                throw new Exception("Nem sikerült elmenteni az alaprajzot!");

            return savedLayout;
        }

        public async Task<Shelf> UpdateShelf(ShelfDto shelf, int id)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/StoreLayout/{id}/shelf", shelf);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }

            var updatedShelf = await response.Content.ReadFromJsonAsync<Shelf>();
            if (updatedShelf == null)
                throw new Exception("Nem sikerült elmenteni a polcot!");

            return updatedShelf;
        }

        public class ProductToShelfResponse
        {
            public Store Store { get; set; }
            public ShelfDto Shelf { get; set; }
        }

        public async Task<(Store store, ShelfDto shelf)> AddProductToShelf(ProductPerStore productPerStore, int shelfId, float d)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/StoreLayout/shelf/{shelfId}/product", new { Pps = productPerStore.ToDto(), D = d });
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            var updatedShelf = await response.Content.ReadFromJsonAsync<ProductToShelfResponse>() ?? throw new Exception("Failed to deserialize the response.");
            return new (updatedShelf.Store, updatedShelf.Shelf);
        }

        public async Task<(Store store, ShelfDto shelf)> RemoveProductFromShelf(ProductPerStore productPerStore, int shelfId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/StoreLayout/shelf/{shelfId}/product/remove", productPerStore.ToDto());
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            var updatedShelf = await response.Content.ReadFromJsonAsync<ProductToShelfResponse>() ?? throw new Exception("Failed to deserialize the response.");
            return new(updatedShelf.Store, updatedShelf.Shelf);
        }

        public async Task<Shelf> GetShelfAsync(int shelfId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Shelf>($"api/StoreLayout/shelf/{shelfId}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
