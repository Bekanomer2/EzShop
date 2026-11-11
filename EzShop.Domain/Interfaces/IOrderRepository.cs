using EzShop.Domain.Entities;

namespace EzShop.Domain.Interfaces;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync();
    Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdAsync(int id);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
}
