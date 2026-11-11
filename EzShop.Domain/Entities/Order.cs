using EzShop.Domain.Enums;

namespace EzShop.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// Проверка допустимости перехода в новый статус (Бизнес-правило предметной области).
    /// Клиент / Бот не могут отменить уже доставленный товар, только Администратор.
    /// </summary>
    public bool CanTransitionTo(OrderStatus newStatus, bool isAdmin = false)
    {
        if (isAdmin) return true;

        return Status switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.InProgress or OrderStatus.Cancelled,
            OrderStatus.InProgress => newStatus is OrderStatus.Delivered or OrderStatus.Returned or OrderStatus.Cancelled,
            OrderStatus.Delivered => newStatus is OrderStatus.Returned,
            OrderStatus.Returned => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }
}
