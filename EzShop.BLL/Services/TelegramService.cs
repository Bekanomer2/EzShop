using System.Text;
using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EzShop.BLL.Services;

public class TelegramService : ITelegramService
{
    private readonly TelegramBotClient? _botClient;
    private readonly string _chatId;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(IConfiguration config, ILogger<TelegramService> logger)
    {
        _logger = logger;
        var token = config["Telegram:BotToken"];
        _chatId = config["Telegram:ChatId"] ?? string.Empty;

        if (!string.IsNullOrEmpty(token) && !token.StartsWith("CHANGE_ME"))
        {
            _botClient = new TelegramBotClient(token);
        }
    }

    public TelegramService(HttpClient httpClient, IConfiguration config, ILogger<TelegramService> logger)
        : this(config, logger)
    {
    }

    public async Task NotifyNewOrderAsync(
        int orderId,
        string username,
        string customerName,
        string customerPhone,
        string customerAddress,
        decimal total,
        IReadOnlyList<CreateOrderItemRequest> items)
    {
        if (_botClient == null || string.IsNullOrEmpty(_chatId))
        {
            _logger.LogWarning("Telegram-бот не настроен (проверьте BotToken и ChatId в appsettings.json).");
            return;
        }

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"🛍 <b>Новый заказ #{orderId} в EzShop!</b>");
            sb.AppendLine();
            sb.AppendLine($"👤 <b>Покупатель:</b> {Escape(customerName)} (@{Escape(username)})");
            sb.AppendLine($"📞 <b>Телефон:</b> {Escape(customerPhone)}");
            sb.AppendLine($"📍 <b>Адрес доставки:</b> {Escape(customerAddress)}");
            sb.AppendLine();
            sb.AppendLine("📦 <b>Состав заказа:</b>");

            foreach (var item in items)
            {
                sb.AppendLine($"  • {Escape(item.Name)} — {item.Qty} шт. × {item.Price:N0} сом");
            }

            sb.AppendLine();
            sb.AppendLine($"💰 <b>Итого к оплате:</b> <b>{total:N0} сом</b>");
            sb.AppendLine($"⏳ <b>Статус:</b> Оформлен");

            // Инлайн-кнопки для нового заказа (В обработке) — только 2 кнопки
            // "🔨 В работу" и "❌ Отменить" с русскими статусами для Supabase Edge Webhook Function
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔨 В работу", $"status:{orderId}:В работе"),
                    InlineKeyboardButton.WithCallbackData("❌ Отменить", $"status:{orderId}:Отменен")
                }
            });

            await _botClient.SendMessage(
                chatId: _chatId,
                text: sb.ToString(),
                parseMode: ParseMode.Html,
                replyMarkup: keyboard
            );

            _logger.LogInformation("Уведомление о заказе #{OrderId} успешно отправлено в Telegram.", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке уведомления о заказе #{OrderId} в Telegram.", orderId);
        }
    }

    public async Task SendAlertAsync(string message)
    {
        if (_botClient == null || string.IsNullOrEmpty(_chatId)) return;

        try
        {
            await _botClient.SendMessage(
                chatId: _chatId,
                text: message,
                parseMode: ParseMode.Html
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке оповещения в Telegram.");
        }
    }

    private static string Escape(string input) =>
        input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
