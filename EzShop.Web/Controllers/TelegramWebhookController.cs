using System.Text.Json;
using EzShop.BLL.Interfaces;
using EzShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

[Route("api/telegram/webhook")]
public class TelegramWebhookController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ILogger<TelegramWebhookController> _logger;

    public TelegramWebhookController(IOrderService orderService, ILogger<TelegramWebhookController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleUpdate([FromBody] JsonElement update)
    {
        try
        {
            if (update.TryGetProperty("callback_query", out var callbackQuery))
            {
                var data = callbackQuery.GetProperty("data").GetString();
                if (!string.IsNullOrEmpty(data) && data.StartsWith("status:"))
                {
                    // Формат: status:{orderId}:{status}
                    var parts = data.Split(':');
                    if (parts.Length == 3 && int.TryParse(parts[1], out var orderId))
                    {
                        var statusStr = parts[2];
                        OrderStatus? parsedStatus = statusStr switch
                        {
                            "В работе" or "InProgress" => OrderStatus.InProgress,
                            "Доставлен" or "Delivered" => OrderStatus.Delivered,
                            "Возврат" or "Returned" => OrderStatus.Returned,
                            "Отменен" or "Cancelled" => OrderStatus.Cancelled,
                            "В обработке" or "Pending" => OrderStatus.Pending,
                            _ => Enum.TryParse<OrderStatus>(statusStr, true, out var st) ? st : null
                        };

                        if (parsedStatus.HasValue)
                        {
                            var status = parsedStatus.Value;
                            _logger.LogInformation("Обработка изменения статуса из Telegram для Заказа #{OrderId} на {Status}", orderId, status);
                            await _orderService.UpdateOrderStatusAsync(orderId, status, isAdmin: false);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке вебхука от Telegram.");
        }

        return Ok();
    }
}
