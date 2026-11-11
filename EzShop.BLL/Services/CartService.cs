using EzShop.BLL.Interfaces;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;

namespace EzShop.BLL.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;

    public CartService(ICartRepository cartRepo)
    {
        _cartRepo = cartRepo;
    }

    public async Task<IReadOnlyList<CartItem>> GetCartAsync(int userId)
    {
        return await _cartRepo.GetByUserIdAsync(userId);
    }

    public async Task AddOrUpdateAsync(int userId, int productId, int qty)
    {
        if (qty <= 0) return;
        await _cartRepo.AddOrUpdateAsync(userId, productId, qty);
    }

    public async Task SetQtyAsync(int userId, int productId, int qty)
    {
        await _cartRepo.SetQtyAsync(userId, productId, qty);
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        await _cartRepo.RemoveItemAsync(userId, productId);
    }

    public async Task ClearCartAsync(int userId)
    {
        await _cartRepo.ClearCartAsync(userId);
    }

    public async Task SyncGuestCartAsync(int userId, IEnumerable<(int productId, int qty)> guestItems)
    {
        foreach (var (productId, qty) in guestItems)
        {
            if (productId > 0 && qty > 0)
            {
                await _cartRepo.AddOrUpdateAsync(userId, productId, qty);
            }
        }
    }
}
