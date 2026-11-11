using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;

namespace EzShop.BLL.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IProductRepository _productRepo;

    public ReviewService(IReviewRepository reviewRepo, IProductRepository productRepo)
    {
        _reviewRepo = reviewRepo;
        _productRepo = productRepo;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await _reviewRepo.GetAllAsync();
        return reviews.Select(r => new ReviewDto(
            r.Id, r.ProductId, r.Author, r.Text, r.Rating, r.CreatedAt
        )).ToList();
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsForProductAsync(int productId)
    {
        var reviews = await _reviewRepo.GetByProductIdAsync(productId);
        return reviews.Select(r => new ReviewDto(
            r.Id, r.ProductId, r.Author, r.Text, r.Rating, r.CreatedAt
        )).ToList();
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var review = await _reviewRepo.GetByIdAsync(id);
        if (review == null) return false;

        var productId = review.ProductId;
        var deleted = await _reviewRepo.DeleteAsync(id);
        if (deleted)
        {
            var remainingReviews = await _reviewRepo.GetByProductIdAsync(productId);
            var product = await _productRepo.GetByIdAsync(productId);
            if (product != null)
            {
                product.Rating = remainingReviews.Any() ? Math.Round(remainingReviews.Average(r => r.Rating), 1) : 5.0;
                await _productRepo.UpdateAsync(product);
            }
        }
        return deleted;
    }

    public async Task<ReviewDto> AddReviewAsync(CreateReviewRequest request)
    {
        var review = new Review
        {
            ProductId = request.ProductId,
            Author = string.IsNullOrWhiteSpace(request.Author) ? "Гость" : request.Author.Trim(),
            Text = request.Text.Trim(),
            Rating = Math.Clamp(request.Rating, 1, 5),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepo.AddAsync(review);

        // Пересчет среднего рейтинга товара
        var allReviews = await _reviewRepo.GetByProductIdAsync(request.ProductId);
        if (allReviews.Any())
        {
            var product = await _productRepo.GetByIdAsync(request.ProductId);
            if (product != null)
            {
                product.Rating = Math.Round(allReviews.Average(r => r.Rating), 1);
                await _productRepo.UpdateAsync(product);
            }
        }

        return new ReviewDto(
            created.Id, created.ProductId, created.Author, created.Text, created.Rating, created.CreatedAt
        );
    }
}
