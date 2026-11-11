using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using EzShop.BLL.Services;
using EzShop.Domain.Entities;
using EzShop.Domain.Enums;
using EzShop.Domain.Interfaces;
using EzShop.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

public class AdminController : Controller
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IBrandRepository _brandRepo;
    private readonly IReviewService _reviewService;

    public AdminController(
        IProductService productService,
        IOrderService orderService,
        IBrandRepository brandRepo,
        IReviewService reviewService)
    {
        _productService = productService;
        _orderService = orderService;
        _brandRepo = brandRepo;
        _reviewService = reviewService;
    }

    [HttpGet]
    [Route("Admin")]
    [Route("admin.html")]
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        var brands = await _brandRepo.GetAllAsync();
        var orders = await _orderService.GetAllOrdersAsync();
        var reviews = await _reviewService.GetAllReviewsAsync();

        var prodDict = products.ToDictionary(p => p.Id, p => p.Name);
        var reviewList = reviews.Select(r => new
        {
            id = r.Id,
            productId = r.ProductId,
            productName = prodDict.TryGetValue(r.ProductId, out var name) ? name : $"Товар #{r.ProductId}",
            author = r.Author,
            rating = r.Rating,
            text = r.Text,
            createdAt = r.CreatedAt.ToString("dd.MM.yyyy HH:mm")
        }).ToList();

        ViewBag.ProductsJson = System.Text.Json.JsonSerializer.Serialize(products, new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        });
        ViewBag.BrandsJson = System.Text.Json.JsonSerializer.Serialize(brands.Select(b => new { id = b.Id, name = b.Name }), new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        });
        ViewBag.ReviewsJson = System.Text.Json.JsonSerializer.Serialize(reviewList, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        var vm = new AdminDashboardViewModel
        {
            Products = products,
            Orders = orders,
            TotalRevenue = orders.Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Returned).Sum(o => o.Total),
            ActiveOrdersCount = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.InProgress),
            OutOfStockCount = products.Count(p => !p.InStock)
        };

        return View(vm);
    }

    [HttpGet]
    [Route("Admin/Orders")]
    [Route("admin-orders.html")]
    [Route("Admin/admin-orders.html")]
    public async Task<IActionResult> Orders()
    {
        var all = await _orderService.GetAllOrdersAsync();
        var model = new AdminOrdersViewModel
        {
            Orders = all,
            TotalCount = all.Count,
            Revenue = all.Sum(o => o.Total),
            InProgressCount = all.Count(o => o.Status is OrderStatus.Pending or OrderStatus.InProgress),
            ReturnsCount = all.Count(o => o.Status is OrderStatus.Returned)
        };
        return View("Orders", model);
    }

    // ==========================================
    //  ЗАКАЗЫ
    // ==========================================

    [HttpGet]
    [Route("Admin/GetOrders")]
    [Route("api/admin/orders")]
    public async Task<IActionResult> GetOrders(string? phone = null, string? status = null, string? sort = null)
    {
        var all = await _orderService.GetAllOrdersAsync();

        var totalCount = all.Count;
        var revenue = all.Sum(o => o.Total);
        var inProgressCount = all.Count(o => o.Status is OrderStatus.Pending or OrderStatus.InProgress);
        var returnsCount = all.Count(o => o.Status is OrderStatus.Returned);

        IEnumerable<OrderDto> filtered = all;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var clean = new string(phone.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(clean))
            {
                filtered = filtered.Where(o => new string(o.CustomerPhone.Where(char.IsDigit).ToArray()).Contains(clean));
            }
        }
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var parsedStatus = OrderService.ParseRussianStatus(status);
            filtered = filtered.Where(o => o.Status == parsedStatus);
        }

        filtered = sort switch
        {
            "date_asc" => filtered.OrderBy(o => o.Id),
            "total_desc" => filtered.OrderByDescending(o => o.Total),
            "total_asc" => filtered.OrderBy(o => o.Total),
            _ => filtered.OrderByDescending(o => o.Id)
        };

        var orderItems = filtered.Select(o => new
        {
            id = o.Id,
            userId = o.UserId,
            username = o.Username,
            customerName = o.CustomerName,
            customerPhone = o.CustomerPhone,
            customerAddress = o.CustomerAddress,
            total = o.Total,
            status = OrderService.GetRussianStatus(o.Status),
            rawStatus = o.Status.ToString(),
            createdAt = o.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            createdDateFormatted = o.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
            items = o.Items.Select(i => new
            {
                productId = i.ProductId,
                name = i.Name,
                price = i.Price,
                qty = i.Qty
            })
        }).ToList();

        return Ok(new
        {
            totalCount,
            revenue,
            inProgressCount,
            returnsCount,
            orders = orderItems
        });
    }

    [HttpPost]
    [Route("Admin/UpdateOrderStatus/{id?}")]
    [Route("api/admin/orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest? req, [FromForm] string? formStatus, [FromQuery] string? queryStatus)
    {
        var rawStatus = req?.Status;
        if (string.IsNullOrWhiteSpace(rawStatus)) rawStatus = formStatus;
        if (string.IsNullOrWhiteSpace(rawStatus)) rawStatus = queryStatus;

        if (id <= 0 && req?.OrderId > 0)
        {
            id = req.OrderId.Value;
        }

        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return BadRequest(new { success = false, message = "Не указан новый статус заказа" });
        }
        var parsed = OrderService.ParseRussianStatus(rawStatus);
        var ok = await _orderService.UpdateOrderStatusAsync(id, parsed, isAdmin: true);
        if (!ok)
        {
            return BadRequest(new { success = false, message = "Не удалось обновить статус заказа" });
        }
        return Ok(new { success = true, newStatus = OrderService.GetRussianStatus(parsed) });
    }

    // ==========================================
    // ТОВАРЫ
    // ==========================================

    [HttpGet]
    [Route("Admin/GetProducts")]
    [Route("api/admin/products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        var brands = await _brandRepo.GetAllAsync();

        return Ok(new
        {
            totalCount = products.Count,
            brandCount = brands.Count,
            inStock = products.Count(p => p.InStock),
            outStock = products.Count(p => !p.InStock),
            products,
            brands
        });
    }

    [HttpPost]
    [Route("Admin/ToggleStock/{id?}")]
    [Route("api/admin/products/{id}/toggle-stock")]
    public async Task<IActionResult> ToggleStock(int id)
    {
        var ok = await _productService.ToggleStockAsync(id);
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(new { success = ok, inStock = product?.InStock ?? false });
    }

    [HttpPost]
    [Route("Admin/UploadImage")]
    [Route("api/admin/upload-image")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Файл не выбран." });
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif" };
        if (!allowed.Contains(ext))
        {
            return BadRequest(new { success = false, message = "Допустимы только форматы изображений: JPG, PNG, WEBP, AVIF." });
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"phone_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..6]}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/{fileName}";
        return Ok(new { success = true, url = relativeUrl });
    }

    [HttpPost]
    [Route("Admin/SaveProduct")]
    [Route("api/admin/products")]
    public async Task<IActionResult> SaveProduct([FromBody] UpsertProductModel model)
    {
        var imgUrl = string.IsNullOrWhiteSpace(model.Img) ? "/img/iPhone 15 pro max.jpg" : model.Img.Trim();
        if (!imgUrl.StartsWith("/") && !imgUrl.StartsWith("http"))
        {
            imgUrl = "/" + imgUrl;
        }

        var req = new UpsertProductRequest(
            model.Name,
            model.Brand,
            model.Price,
            imgUrl,
            model.Ram,
            model.Storage,
            model.Color,
            model.Display,
            model.Battery,
            model.Description,
            model.InStock
        );

        if (model.Id > 0)
        {
            var updated = await _productService.UpdateProductAsync(model.Id, req);
            return Ok(new { success = true, product = updated });
        }
        else
        {
            var created = await _productService.CreateProductAsync(req);
            return Ok(new { success = true, product = created });
        }
    }

    [HttpPost]
    [HttpDelete]
    [Route("Admin/DeleteProduct/{id?}")]
    [Route("api/admin/products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var ok = await _productService.DeleteProductAsync(id);
        return Ok(new { success = ok });
    }

    // ==========================================
    // 🏷️ C# MVC ЭКШЕНЫ: БРЕНДЫ
    // ==========================================

    [HttpGet]
    [Route("Admin/GetBrands")]
    [Route("api/admin/brands")]
    public async Task<IActionResult> GetBrands()
    {
        var brands = await _brandRepo.GetAllAsync();
        return Ok(brands);
    }

    [HttpPost]
    [Route("Admin/SaveBrand")]
    [Route("api/admin/brands")]
    public async Task<IActionResult> SaveBrand([FromBody] BrandModel model)
    {
        var b = new Brand
        {
            Name = model.Name.Trim(),
            Country = model.Country.Trim()
        };
        var created = await _brandRepo.AddAsync(b);
        return Ok(new { success = true, brand = created });
    }

    [HttpPost]
    [HttpDelete]
    [Route("Admin/DeleteBrand/{id?}")]
    [Route("api/admin/brands/{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var ok = await _brandRepo.DeleteAsync(id);
        return Ok(new { success = ok });
    }

    // ==========================================
    //  ОТЗЫВЫ
    // ==========================================

    [HttpGet]
    [Route("Admin/GetReviews")]
    [Route("api/admin/reviews")]
    public async Task<IActionResult> GetReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        var products = await _productService.GetAllProductsAsync();
        var prodDict = products.ToDictionary(p => p.Id, p => p.Name);

        var list = reviews.Select(r => new
        {
            id = r.Id,
            productId = r.ProductId,
            productName = prodDict.TryGetValue(r.ProductId, out var name) ? name : $"Товар #{r.ProductId}",
            author = r.Author,
            rating = r.Rating,
            text = r.Text,
            createdAt = r.CreatedAt.ToString("dd.MM.yyyy HH:mm")
        });

        return Ok(list);
    }

    [HttpPost]
    [HttpDelete]
    [Route("Admin/DeleteReview/{id:int?}")]
    [Route("api/admin/reviews/{id:int?}")]
    public async Task<IActionResult> DeleteReview(int? id, [FromQuery] int? reviewId)
    {
        var targetId = id ?? reviewId ?? 0;
        if (targetId <= 0) return BadRequest(new { success = false, message = "Не указан ID отзыва" });

        var isAdmin = User.IsInRole("Admin") || string.Equals(User.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin) return Forbid();

        var ok = await _reviewService.DeleteReviewAsync(targetId);
        return Ok(new { success = ok });
    }
}

public class UpdateStatusRequest
{
    public int? OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpsertProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Img { get; set; } = string.Empty;
    public int Ram { get; set; }
    public int Storage { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public int Battery { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool InStock { get; set; } = true;
}

public class BrandModel
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}