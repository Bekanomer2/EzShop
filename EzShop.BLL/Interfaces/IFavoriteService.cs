namespace EzShop.BLL.Interfaces;

public interface IFavoriteService
{
    Task<IReadOnlyList<int>> GetFavoriteProductIdsAsync(int userId);
    Task<bool> ToggleFavoriteAsync(int userId, int productId);
    Task<bool> AddFavoriteAsync(int userId, int productId);
    Task<bool> RemoveFavoriteAsync(int userId, int productId);
}
