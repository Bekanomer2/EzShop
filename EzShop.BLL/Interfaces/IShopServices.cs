using EzShop.BLL.DTO;
using EzShop.Domain.Enums;

namespace EzShop.BLL.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(UpsertProductRequest request);
    Task<ProductDto?> UpdateProductAsync(int id, UpsertProductRequest request);
    Task<bool> ToggleStockAsync(int id);
    Task<bool> DeleteProductAsync(int id);
}

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(int userId);
    Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> CreateOrderAsync(int userId, string username, CreateOrderRequest request);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, bool isAdmin = false);
}

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetAllReviewsAsync();
    Task<IReadOnlyList<ReviewDto>> GetReviewsForProductAsync(int productId);
    Task<ReviewDto> AddReviewAsync(CreateReviewRequest request);
    Task<bool> DeleteReviewAsync(int id);
}

public interface ITelegramService
{
    Task NotifyNewOrderAsync(
        int orderId,
        string username,
        string customerName,
        string customerPhone,
        string customerAddress,
        decimal total,
        IReadOnlyList<CreateOrderItemRequest> items
    );

    Task SendAlertAsync(string message);
}
