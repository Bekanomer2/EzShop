using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using EzShop.Domain.Entities;
using EzShop.Domain.Enums;
using EzShop.Domain.Interfaces;

namespace EzShop.BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ITelegramService _telegram;

    public OrderService(IOrderRepository orderRepo, ITelegramService telegram)
    {
        _orderRepo = orderRepo;
        _telegram = telegram;
    }

    public async Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(int userId)
    {
        if (userId <= 0) return Array.Empty<OrderDto>();
        var orders = await _orderRepo.GetByUserIdAsync(userId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepo.GetAllAsync();
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, string username, CreateOrderRequest request)
    {
        var total = request.Items.Sum(i => i.Price * i.Qty);

        var order = new Order
        {
            UserId = userId,
            Username = username,
            Total = total,
            Status = OrderStatus.Pending,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerAddress = request.CustomerAddress,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Name = i.Name,
                Price = i.Price,
                Qty = i.Qty
            }).ToList()
        };

        var created = await _orderRepo.AddAsync(order);

        // Уведомление в Telegram-бот
        try
        {
            await _telegram.NotifyNewOrderAsync(
                created.Id,
                created.Username,
                created.CustomerName,
                created.CustomerPhone,
                created.CustomerAddress,
                created.Total,
                request.Items
            );
        }
        catch { }

        return MapToDto(created);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, bool isAdmin = false)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order is null) return false;

        if (!order.CanTransitionTo(newStatus, isAdmin))
        {
            try
            {
                await _telegram.SendAlertAsync(
                    $"⚠️ <b>Отклонено изменение статуса Заказа #{order.Id}</b>\n\n" +
                    $"Нельзя перевести заказ со статуса «{GetRussianStatus(order.Status)}» в «{GetRussianStatus(newStatus)}».\n" +
                    $"<i>Изменить ошибочный статус может только Администратор на сайте.</i>"
                );
            }
            catch { }
            return false;
        }

        order.Status = newStatus;
        await _orderRepo.UpdateAsync(order);

        try
        {
            await _telegram.SendAlertAsync($"ℹ️ <b>Статус заказа #{order.Id} изменен на: {GetRussianStatus(newStatus)}</b>");
        }
        catch { }

        return true;
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id,
        o.UserId,
        o.Username,
        o.Total,
        o.Status,
        GetRussianStatus(o.Status),
        o.CustomerName,
        o.CustomerPhone,
        o.CustomerAddress,
        o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.Name, i.Price, i.Qty)).ToList()
    );

    public static string GetRussianStatus(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "В обработке",
        OrderStatus.InProgress => "В работе",
        OrderStatus.Delivered => "Доставлен",
        OrderStatus.Returned => "Возврат",
        OrderStatus.Cancelled => "Отменен",
        _ => "Неизвестно"
    };

    public static OrderStatus ParseRussianStatus(string status) => status switch
    {
        "В работе" or "InProgress" => OrderStatus.InProgress,
        "Доставлен" or "Delivered" => OrderStatus.Delivered,
        "Возврат" or "Returned" => OrderStatus.Returned,
        "Отменен" or "Cancelled" => OrderStatus.Cancelled,
        _ => OrderStatus.Pending
    };
}
