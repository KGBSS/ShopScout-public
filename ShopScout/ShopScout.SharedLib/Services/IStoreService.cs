using ShopScout.SharedLib.Models;

namespace ShopScout.SharedLib.Services
{
    public interface IStoreService
    {
        Task<Store?> GetStoreAsync(int id);
        Task<List<Store>> SearchStore(string searchText);
        Task<Store?> AdminGetStoreAsync(int id);
        Task<bool> AdminUpdateStoreAsync(Store store);
        Task AdminDeleteStoreAsync(Store store);
        Task AttachProducts(Store store);
        Task MergeOSM();
    }
}