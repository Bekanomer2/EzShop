using EzShop.Domain.Entities;

namespace EzShop.Domain.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetAllAsync();
    Task<IReadOnlyList<Review>> GetByProductIdAsync(int productId);
    Task<Review?> GetByIdAsync(int id);
    Task<Review> AddAsync(Review review);
    Task<bool> DeleteAsync(int id);
}
