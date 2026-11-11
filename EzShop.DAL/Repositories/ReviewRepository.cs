using EzShop.DAL.Context;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EzShop.DAL.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly EzShopDbContext _context;

    public ReviewRepository(EzShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(int productId)
    {
        return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Review> AddAsync(Review review)
    {
        review.Id = 0;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }
}
