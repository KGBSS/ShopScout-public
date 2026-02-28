using ShopScout.SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services
{
    public class ClientStoreService : IStoreService
    {
        private readonly HttpClient _httpClient;
        public ClientStoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task AdminDeleteStoreAsync(Store store)
        {
            throw new NotImplementedException();
        }

        public Task<Store?> AdminGetStoreAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AdminUpdateStoreAsync(Store store)
        {
            throw new NotImplementedException();
        }

        public Task AttachProducts(Store store)
        {
            throw new NotImplementedException();
        }

        public async Task<Store?> GetStoreAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Store>($"api/Store/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public Task MergeOSM()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Store>> SearchStore(string searchText)
        {
            return await _httpClient.GetFromJsonAsync<List<Store>>($"api/Store/search/{searchText}");
        }
    }
}
