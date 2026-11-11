using EzShop.Domain.Entities;

namespace EzShop.BLL.Interfaces;

public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetCartAsync(int userId);
    Task AddOrUpdateAsync(int userId, int productId, int qty);
    Task SetQtyAsync(int userId, int productId, int qty);
    Task RemoveItemAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
    Task SyncGuestCartAsync(int userId, IEnumerable<(int productId, int qty)> guestItems);
}
