namespace EzShop.Domain.Interfaces;

public interface ICartRepository
{
    Task<IReadOnlyList<Entities.CartItem>> GetByUserIdAsync(int userId);
    Task<Entities.CartItem?> GetItemAsync(int userId, int productId);
    Task AddOrUpdateAsync(int userId, int productId, int qty);
    Task SetQtyAsync(int userId, int productId, int qty);
    Task RemoveItemAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}
