using EzShop.DAL.Context;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EzShop.DAL.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly EzShopDbContext _context;

    public FavoriteRepository(EzShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<int>> GetProductIdsByUserIdAsync(int userId)
    {
        if (userId <= 0) return Array.Empty<int>();

        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.ProductId)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(int userId, int productId)
    {
        if (userId <= 0 || productId <= 0) return false;

        var exists = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        if (exists) return true;

        var fav = new UserFavorite
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favorites.Add(fav);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(int userId, int productId)
    {
        if (userId <= 0 || productId <= 0) return false;

        var fav = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        if (fav is null) return false;

        _context.Favorites.Remove(fav);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int userId, int productId)
    {
        if (userId <= 0 || productId <= 0) return false;
        return await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
    }
}
