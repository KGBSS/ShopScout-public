using Microsoft.EntityFrameworkCore;
using ShopScout.Data;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;

namespace ShopScout.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    public CategoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ProductCategory>> GetAll()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.ProductCategories.ToListAsync();
    }
    
    public async Task<List<ProductCategory>> GetAllBottomLayer()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.ProductCategories.Where(c => !c.SubCategories.Any() && c.Verified == true).ToListAsync();
    }


    public async Task<ProductCategory> GetById(int id)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.ProductCategories.Include(c => c.Products)
                                               .ThenInclude(p => p.ProductImages)
                                               .Include(c => c.SubCategories)
                                               .Include(c => c.ParentCategory)
                                               .FirstOrDefaultAsync(c => c.Id == id);
    }
}