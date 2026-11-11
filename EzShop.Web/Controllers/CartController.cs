using System.Security.Claims;
using EzShop.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

public class CartController : Controller
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public CartController(IProductService productService, ICartService cartService)
    {
        _productService = productService;
        _cartService = cartService;
    }

    [HttpGet]
    [Route("Cart")]
    [Route("cart.html")]
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        ViewBag.ProductsJson = System.Text.Json.JsonSerializer.Serialize(products, new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        });
        return View();
    }

    [HttpGet]
    [Route("Cart/GetMyCart")]
    [Route("api/cart")]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, isAuth = false, items = Array.Empty<object>() });
        }

        var cartItems = await _cartService.GetCartAsync(userId);
        var payload = cartItems.Select(c => new
        {
            id = c.ProductId,
            qty = c.Qty,
            name = c.Product?.Name ?? string.Empty,
            price = c.Product?.Price ?? 0m,
            img = c.Product?.Img ?? string.Empty,
            brand = c.Product?.Brand ?? string.Empty,
            storage = c.Product?.Storage ?? 0,
            ram = c.Product?.Ram ?? 0,
            color = c.Product?.Color ?? string.Empty,
            inStock = c.Product?.InStock ?? true
        });

        return Ok(new { success = true, isAuth = true, items = payload });
    }

    [HttpPost]
    [Route("Cart/AddItem")]
    [Route("api/cart/add")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddItem([FromBody] CartItemRequest req)
    {
        if (req.ProductId <= 0) return BadRequest(new { success = false, message = "Не указан ID товара" });

        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, isAuth = false, productId = req.ProductId, qty = req.Qty });
        }

        await _cartService.AddOrUpdateAsync(userId, req.ProductId, req.Qty <= 0 ? 1 : req.Qty);
        return Ok(new { success = true, isAuth = true, productId = req.ProductId });
    }

    [HttpPost]
    [Route("Cart/SetQty")]
    [Route("api/cart/set-qty")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SetQty([FromBody] CartItemRequest req)
    {
        if (req.ProductId <= 0) return BadRequest(new { success = false, message = "Не указан ID товара" });

        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, isAuth = false, productId = req.ProductId, qty = req.Qty });
        }

        await _cartService.SetQtyAsync(userId, req.ProductId, req.Qty);
        return Ok(new { success = true, isAuth = true, productId = req.ProductId, qty = req.Qty });
    }

    [HttpPost]
    [Route("Cart/RemoveItem")]
    [Route("api/cart/remove")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RemoveItem([FromBody] CartItemRequest req)
    {
        if (req.ProductId <= 0) return BadRequest(new { success = false, message = "Не указан ID товара" });

        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, isAuth = false, productId = req.ProductId });
        }

        await _cartService.RemoveItemAsync(userId, req.ProductId);
        return Ok(new { success = true, isAuth = true, productId = req.ProductId });
    }

    [HttpPost]
    [Route("Cart/Clear")]
    [Route("api/cart/clear")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Clear()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await _cartService.ClearCartAsync(userId);
        }
        return Ok(new { success = true, isAuth = userId > 0 });
    }

    [HttpPost]
    [Route("Cart/SyncGuestCart")]
    [Route("api/cart/sync")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SyncGuestCart([FromBody] SyncGuestCartRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, synced = false, message = "Пользователь не авторизован" });
        }

        if (req?.Items != null && req.Items.Count > 0)
        {
            var pairs = req.Items.Select(x => (x.ProductId, x.Qty));
            await _cartService.SyncGuestCartAsync(userId, pairs);
        }

        var cartItems = await _cartService.GetCartAsync(userId);
        var payload = cartItems.Select(c => new
        {
            id = c.ProductId,
            qty = c.Qty,
            name = c.Product?.Name ?? string.Empty,
            price = c.Product?.Price ?? 0m,
            img = c.Product?.Img ?? string.Empty,
            brand = c.Product?.Brand ?? string.Empty,
            storage = c.Product?.Storage ?? 0,
            ram = c.Product?.Ram ?? 0,
            color = c.Product?.Color ?? string.Empty,
            inStock = c.Product?.InStock ?? true
        });

        return Ok(new { success = true, synced = true, items = payload });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}

public class CartItemRequest
{
    public int ProductId { get; set; }
    public int Qty { get; set; } = 1;
}

public class SyncGuestCartRequest
{
    public List<CartItemRequest> Items { get; set; } = new();
}
