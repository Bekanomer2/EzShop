using EzShop.BLL.Interfaces;
using EzShop.Domain.Interfaces;

namespace EzShop.BLL.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepo;

    public FavoriteService(IFavoriteRepository favoriteRepo)
    {
        _favoriteRepo = favoriteRepo;
    }

    public async Task<IReadOnlyList<int>> GetFavoriteProductIdsAsync(int userId)
    {
        return await _favoriteRepo.GetProductIdsByUserIdAsync(userId);
    }

    public async Task<bool> ToggleFavoriteAsync(int userId, int productId)
    {
        if (userId <= 0 || productId <= 0) return false;

        var exists = await _favoriteRepo.ExistsAsync(userId, productId);
        if (exists)
        {
            await _favoriteRepo.RemoveAsync(userId, productId);
            return false; // Теперь убран из избранного
        }
        else
        {
            await _favoriteRepo.AddAsync(userId, productId);
            return true; // Теперь добавлен в избранное
        }
    }

    public async Task<bool> AddFavoriteAsync(int userId, int productId)
    {
        return await _favoriteRepo.AddAsync(userId, productId);
    }

    public async Task<bool> RemoveFavoriteAsync(int userId, int productId)
    {
        return await _favoriteRepo.RemoveAsync(userId, productId);
    }
}
