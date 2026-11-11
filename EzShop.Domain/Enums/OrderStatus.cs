namespace EzShop.Domain.Enums;

/// <summary>
/// Статусы жизненного цикла заказа в интернет-магазине.
/// </summary>
public enum OrderStatus
{
    /// <summary>Оформлен клиентом, ожидает подтверждения</summary>
    Pending,

    /// <summary>В работе / сборке на складе</summary>
    InProgress,

    /// <summary>Успешно доставлен клиенту</summary>
    Delivered,

    /// <summary>Возврат товара</summary>
    Returned,

    /// <summary>Отменен клиентом или менеджером</summary>
    Cancelled
}
