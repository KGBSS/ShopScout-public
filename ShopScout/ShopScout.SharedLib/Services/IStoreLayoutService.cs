using ShopScout.SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services
{
    public interface IStoreLayoutService
    {
        Task<Store?> GetStoreAsync(int id);

        Task<Shelf> UpdateShelf(ShelfDto shelf, int id);

        Task<Store> StoreWalls(LayoutDto wallsJson, int id);

        Task<(Store store, ShelfDto shelf)> AddProductToShelf(ProductPerStore productPerStore, int shelfId, float d);

        Task<(Store store, ShelfDto shelf)> RemoveProductFromShelf(ProductPerStore productPerStore, int shelfId);

        Task<Shelf> GetShelfAsync(int shelfId);
    }
}
