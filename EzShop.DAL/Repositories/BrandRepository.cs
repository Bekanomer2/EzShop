using EzShop.DAL.Context;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EzShop.DAL.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly EzShopDbContext _context;

    public BrandRepository(EzShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Brand>> GetAllAsync()
    {
        return await _context.Brands.OrderBy(b => b.Name).AsNoTracking().ToListAsync();
    }

    public async Task<Brand?> GetByIdAsync(int id)
    {
        return await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Brand> AddAsync(Brand brand)
    {
        brand.Id = 0;
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        return brand;
    }

    public async Task UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand != null)
        {
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
