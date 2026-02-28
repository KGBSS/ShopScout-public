using Microsoft.EntityFrameworkCore;
using ShopScout.Data;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;

namespace ShopScout.Services
{
    public class StoreLayoutService : IStoreLayoutService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _context;
        public StoreLayoutService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _context = contextFactory;
        }

        public async Task<Store?> GetStoreAsync(int id)
        {
            using var context = _context.CreateDbContext();

            var store = await context.Stores
                .Include(s => s.Layout)
                    .ThenInclude(l => l.Walls)
                .Include(s => s.Layout)
                    .ThenInclude(l => l.Shelves)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null) return null;

            var allProductsStore = await context.ProductPerStore
                .Where(pps => pps.StoreId == id)
                .Include(pps => pps.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(pps => pps.Product)
                    .ThenInclude(p => p.Categories)
                .ToListAsync();

            if (store.Layout?.Shelves != null)
            {
                foreach (var shelf in store.Layout.Shelves)
                {
                    shelf.Products = allProductsStore
                        .Where(p => p.ShelfId == shelf.Id)
                        .ToList();
                }
            }

            store.ProductPerStore = allProductsStore;

            return store;
        }

        public async Task<Store> StoreWalls(LayoutDto layoutDto, int id)
        {
            using var context = _context.CreateDbContext();

            var store = await context.Stores
                .Include(s => s.Layout)
                    .ThenInclude(l => l.Walls)
                .Include(s => s.Layout)
                    .ThenInclude(l => l.Shelves)
                .Include(s => s.ProductPerStore)
                .FirstOrDefaultAsync(s => s.Id == id) ?? throw new Exception("A bolt nem található!");

            var layout = layoutDto.ToEntity(id);
            var dbLayout = store.Layout ??= new LayoutObject { StoreId = id };

            dbLayout.EntranceX1 = layout.EntranceX1;
            dbLayout.EntranceY1 = layout.EntranceY1;
            dbLayout.EntranceX2 = layout.EntranceX2;
            dbLayout.EntranceY2 = layout.EntranceY2;

            var clientWallIds = layout.Walls.Where(w => w.Id != 0).Select(w => w.Id).ToList();
            var wallsToRemove = dbLayout.Walls.Where(w => !clientWallIds.Contains(w.Id)).ToList();
            if (wallsToRemove.Any()) context.Walls.RemoveRange(wallsToRemove);

            foreach (var wall in layout.Walls)
            {
                if (wall.Id == 0)
                {
                    dbLayout.Walls.Add(new Wall { X1 = wall.X1, Y1 = wall.Y1, X2 = wall.X2, Y2 = wall.Y2 });
                }
                else
                {
                    var dbWall = dbLayout.Walls.FirstOrDefault(w => w.Id == wall.Id);
                    if (dbWall != null)
                    {
                        dbWall.X1 = wall.X1; dbWall.Y1 = wall.Y1; dbWall.X2 = wall.X2; dbWall.Y2 = wall.Y2;
                    }
                }
            }

            var clientShelfIds = layout.Shelves.Where(s => s.Id != 0).Select(s => s.Id).ToList();
            var shelvesToRemove = dbLayout.Shelves.Where(s => !clientShelfIds.Contains(s.Id)).ToList();
            if (shelvesToRemove.Any()) context.Shelves.RemoveRange(shelvesToRemove);

            foreach (var shelf in layout.Shelves)
            {
                if (shelf.Id == 0)
                {
                    dbLayout.Shelves.Add(new Shelf { X1 = shelf.X1, Y1 = shelf.Y1, X2 = shelf.X2, Y2 = shelf.Y2, Type = shelf.Type, Side = shelf.Side, Products = [] });
                }
                else
                {
                    var dbShelf = dbLayout.Shelves.FirstOrDefault(s => s.Id == shelf.Id);
                    if (dbShelf != null)
                    {
                        dbShelf.X1 = shelf.X1; dbShelf.Y1 = shelf.Y1; dbShelf.X2 = shelf.X2; dbShelf.Y2 = shelf.Y2;
                        dbShelf.Type = shelf.Type; dbShelf.Side = shelf.Side;
                    }
                }
            }

            await context.SaveChangesAsync();

            return await GetStoreAsync(id) ?? store;
        }

        public async Task<Shelf> UpdateShelf(ShelfDto shelf, int id)
        {
            if (int.TryParse(shelf.Id, out int shelfId) == false)
            {
                throw new ArgumentException("Érvénytelen polc azonosító!");
            }

            using var context = _context.CreateDbContext();
            Shelf? dbShelf = await context.Shelves
                .Include(s => s.LayoutObject)
                .Include(s => s.Products)
                    .ThenInclude(pps => pps.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(s => s.Id.ToString() == shelf.Id && s.LayoutObject.StoreId == id) ?? throw new KeyNotFoundException("A polc nem talalálható a megadott boltban!");

            dbShelf = shelf.ToEntity(dbShelf);

            context.Shelves.Update(dbShelf);

            await context.SaveChangesAsync();

            return dbShelf;
        }

        public async Task<(Store store, ShelfDto shelf)> AddProductToShelf(ProductPerStore productPerStore, int shelfId, float d)
        {
            using var context = _context.CreateDbContext();
            var existing = await context.ProductPerStore
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productPerStore.ProductId &&
                                          x.StoreId == productPerStore.StoreId);
            if (existing == null)
            {
                productPerStore.Product = null!;
                context.Add(productPerStore);
            }
            else
            {
                context.ProductPerStore.Attach(productPerStore);
            }

            productPerStore.ShelfId = shelfId;
            productPerStore.DistanceFromP1 = d;
            await context.SaveChangesAsync();

            var store = await GetStoreAsync(productPerStore.StoreId) ?? throw new Exception("Bolt nem található!");

            var shelf = await context.Shelves
                .Include(s => s.Products)
                    .ThenInclude(pps => pps.Product)
                .FirstOrDefaultAsync(s => s.Id == shelfId) ?? throw new Exception("Polc nem található!");

            return new(store, shelf.ToDto());
        }

        public async Task<(Store store, ShelfDto shelf)> RemoveProductFromShelf(ProductPerStore productPerStore, int shelfId)
        {
            using var context = _context.CreateDbContext();

            context.ProductPerStore.Attach(productPerStore);
            productPerStore.ShelfId = null;
            productPerStore.DistanceFromP1 = null;
            await context.SaveChangesAsync();

            var store = await GetStoreAsync(productPerStore.StoreId) ?? throw new Exception("Bolt nem található!");

            var shelf = await context.Shelves
                .Include(s => s.Products)
                    .ThenInclude(pps => pps.Product)
                .FirstOrDefaultAsync(s => s.Id == shelfId) ?? throw new Exception("Polc nem található!");
            
            return new(store, shelf.ToDto());
        }

        public async Task<Shelf> GetShelfAsync(int shelfId)
        {
            using var context = _context.CreateDbContext();
            var shelf = await context.Shelves
                .Include(s => s.Products)
                    .ThenInclude(pps => pps.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(s => s.Id == shelfId);
            return shelf ?? throw new KeyNotFoundException("A polc nem található!");
        }
    }
}
