namespace EzShop.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<int>> GetProductIdsByUserIdAsync(int userId);
    Task<bool> AddAsync(int userId, int productId);
    Task<bool> RemoveAsync(int userId, int productId);
    Task<bool> ExistsAsync(int userId, int productId);
}
