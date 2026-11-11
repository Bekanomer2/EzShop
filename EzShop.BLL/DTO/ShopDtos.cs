using EzShop.Domain.Enums;

namespace EzShop.BLL.DTO;

public record ProductDto(
    int Id,
    string Name,
    string Brand,
    decimal Price,
    string Img,
    int Ram,
    int Storage,
    string Color,
    string Display,
    int Battery,
    string Description,
    bool InStock,
    double Rating = 5.0
);

public record UpsertProductRequest(
    string Name,
    string Brand,
    decimal Price,
    string Img,
    int Ram,
    int Storage,
    string Color,
    string Display,
    int Battery,
    string Description,
    bool InStock
);

public record OrderItemDto(
    int ProductId,
    string Name,
    decimal Price,
    int Qty
);

public record OrderDto(
    int Id,
    int UserId,
    string Username,
    decimal Total,
    OrderStatus Status,
    string StatusText,
    string CustomerName,
    string CustomerPhone,
    string CustomerAddress,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items
);

public record CreateOrderItemRequest(
    int ProductId,
    string Name,
    decimal Price,
    int Qty
);

public record CreateOrderRequest(
    string CustomerName,
    string CustomerPhone,
    string CustomerAddress,
    List<CreateOrderItemRequest> Items
);

public record ReviewDto(
    int Id,
    int ProductId,
    string Author,
    string Text,
    int Rating,
    DateTime CreatedAt
);

public record CreateReviewRequest(
    int ProductId,
    string Author,
    string Text,
    int Rating
);
