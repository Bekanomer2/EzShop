using EzShop.DAL.Context;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EzShop.DAL.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly EzShopDbContext _context;

    public OrderRepository(EzShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId)
    {
        if (userId <= 0) return Array.Empty<Order>();
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        order.Id = 0;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
