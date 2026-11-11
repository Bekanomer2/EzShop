using EzShop.DAL.Context;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EzShop.DAL.Repositories;

public class CartRepository : ICartRepository
{
    private readonly EzShopDbContext _context;

    public CartRepository(EzShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CartItem>> GetByUserIdAsync(int userId)
    {
        return await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<CartItem?> GetItemAsync(int userId, int productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task AddOrUpdateAsync(int userId, int productId, int qty)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
        {
            existing.Qty = Math.Clamp(existing.Qty + qty, 1, 10);
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            await _context.CartItems.AddAsync(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Qty = Math.Clamp(qty, 1, 10),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetQtyAsync(int userId, int productId, int qty)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
        {
            if (qty <= 0)
            {
                _context.CartItems.Remove(existing);
            }
            else
            {
                existing.Qty = Math.Clamp(qty, 1, 10);
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
        else if (qty > 0)
        {
            await _context.CartItems.AddAsync(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Qty = Math.Clamp(qty, 1, 10),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
        {
            _context.CartItems.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (items.Count > 0)
        {
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
