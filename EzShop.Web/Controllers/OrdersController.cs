using System.Security.Claims;
using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using EzShop.BLL.Services;
using EzShop.Domain.Entities;
using EzShop.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly UserManager<User> _userManager;

    public OrdersController(IOrderService orderService, IProductService productService, UserManager<User> userManager)
    {
        _orderService = orderService;
        _productService = productService;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Orders")]
    [Route("orders.html")]
    public async Task<IActionResult> Index()
    {
        var isAuth = User.Identity?.IsAuthenticated == true;
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name ?? "Гость";
        var isAdmin = isAuth && (User.IsInRole("Admin") || string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase));

        var orders = isAuth
            ? (isAdmin ? await _orderService.GetAllOrdersAsync() : await _orderService.GetUserOrdersAsync(userId))
            : Array.Empty<OrderDto>();

        var vm = new OrdersIndexViewModel
        {
            UserId = userId,
            Username = username,
            Orders = orders
        };

        return View(vm);
    }

    [HttpGet]
    [Route("Orders/GetMyOrders")]
    [Route("api/orders/my")]
    public async Task<IActionResult> GetMyOrders()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Json(new { success = true, orders = Array.Empty<object>() });
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin") || string.Equals(User.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);

        var orders = isAdmin
            ? await _orderService.GetAllOrdersAsync()
            : await _orderService.GetUserOrdersAsync(userId);

        var result = orders.Select(o => new
        {
            id = o.Id,
            total = o.Total,
            status = o.StatusText,
            rawStatus = o.Status.ToString(),
            date = o.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
            customerName = o.CustomerName,
            customerPhone = o.CustomerPhone,
            customerAddress = o.CustomerAddress,
            items = o.Items.Select(i => new { id = i.ProductId, name = i.Name, price = i.Price, qty = i.Qty })
        });

        return Json(new { success = true, orders = result });
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Route("Orders/CreateDirect")]
    public async Task<IActionResult> CreateDirect(int productId, string customerName, string customerPhone, string customerAddress)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            if (Request.Headers["Accept"].ToString().Contains("application/json") || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, message = "Выбранный товар не найден." });
            }
            TempData["ErrorMessage"] = "Выбранный товар не найден.";
            return RedirectToAction("Index", "Catalog");
        }

        if (!product.InStock)
        {
            if (Request.Headers["Accept"].ToString().Contains("application/json") || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, message = "К сожалению, этот товар сейчас отсутствует на складе." });
            }
            TempData["ErrorMessage"] = "К сожалению, этот товар сейчас отсутствует на складе.";
            return RedirectToAction("Details", "Catalog", new { id = productId });
        }

        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(customerAddress))
        {
            if (Request.Headers["Accept"].ToString().Contains("application/json") || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, message = "Пожалуйста, заполните все обязательные поля (Имя, Телефон, Адрес)." });
            }
            TempData["ErrorMessage"] = "Пожалуйста, заполните все обязательные поля (Имя, Телефон, Адрес).";
            return RedirectToAction("Details", "Catalog", new { id = productId });
        }

        if (!PhoneValidator.IsValid(customerPhone))
        {
            if (Request.Headers["Accept"].ToString().Contains("application/json") || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, message = "Номер телефона должен быть в формате Кыргызстана (+996XXXXXXXXX или 0XXXXXXXXX)." });
            }
            TempData["ErrorMessage"] = "Номер телефона должен быть в формате Кыргызстана (+996XXXXXXXXX или 0XXXXXXXXX).";
            return RedirectToAction("Details", "Catalog", new { id = productId });
        }

        var normalizedPhone = PhoneValidator.Normalize(customerPhone);
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name ?? customerName.Trim();

        var request = new CreateOrderRequest(
            customerName.Trim(),
            normalizedPhone,
            customerAddress.Trim(),
            new List<CreateOrderItemRequest>
            {
                new(product.Id, product.Name, product.Price, 1)
            }
        );

        var created = await _orderService.CreateOrderAsync(userId, username, request);

        if (Request.Headers["Accept"].ToString().Contains("application/json") || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Ok(new { success = true, orderId = created.Id, message = $"Заказ #{created.Id} успешно оформлен!" });
        }

        TempData["SuccessMessage"] = $"Заказ #{created.Id} успешно оформлен! Менеджер свяжется с вами в ближайшее время.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("Orders/CreateFromCart")]
    [Route("api/orders/cart")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateFromCart([FromBody] CreateCartOrderRequest request)
    {
        if (request == null || request.Items == null || !request.Items.Any())
        {
            return BadRequest(new { success = false, message = "Корзина пуста." });
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.CustomerPhone) || string.IsNullOrWhiteSpace(request.CustomerAddress))
        {
            return BadRequest(new { success = false, message = "Заполните все контактные данные (имя, телефон, адрес доставки)." });
        }

        if (!PhoneValidator.IsValid(request.CustomerPhone))
        {
            return BadRequest(new { success = false, message = "Номер телефона должен быть в формате Кыргызстана (+996XXXXXXXXX или 0XXXXXXXXX)." });
        }

        var normalizedPhone = PhoneValidator.Normalize(request.CustomerPhone);
        var userId = GetCurrentUserId();
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            username = request.CustomerName.Trim();
        }

        var orderItems = request.Items.Select(i => new CreateOrderItemRequest(i.Id, i.Name, i.Price, i.Qty)).ToList();

        var orderReq = new CreateOrderRequest(
            request.CustomerName.Trim(),
            normalizedPhone,
            request.CustomerAddress.Trim(),
            orderItems
        );

        var created = await _orderService.CreateOrderAsync(userId, username, orderReq);

        return Ok(new { success = true, orderId = created.Id });
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : 0;
    }
}

public class CreateCartOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public List<CartItemPayload> Items { get; set; } = new();
}

public class CartItemPayload
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Qty { get; set; }
}
