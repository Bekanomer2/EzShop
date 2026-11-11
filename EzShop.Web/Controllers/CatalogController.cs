using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using EzShop.Domain.Entities;
using EzShop.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

public class CatalogController : Controller
{
    private readonly IProductService _productService;
    private readonly IReviewService _reviewService;

    public CatalogController(IProductService productService, IReviewService reviewService)
    {
        _productService = productService;
        _reviewService = reviewService;
    }

    [HttpGet]
    [Route("")]
    [Route("index.html")]
    [Route("Catalog")]
    [Route("Catalog/Index")]
    public async Task<IActionResult> Index(string? brand, string? search, string? sort)
    {
        var all = await _productService.GetAllProductsAsync();

        var query = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => string.Equals(p.Brand, brand, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(p => p.Name.ToLowerInvariant().Contains(s) || p.Brand.ToLowerInvariant().Contains(s));
        }

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.InStock).ThenBy(p => p.Id)
        };

        var brands = all.Select(p => p.Brand).Distinct().OrderBy(b => b).ToList();

        var vm = new CatalogIndexViewModel
        {
            Products = query.ToList(),
            Brands = brands,
            SelectedBrand = brand,
            SearchQuery = search,
            SortBy = sort
        };

        return View(vm);
    }

    [HttpGet]
    [Route("api/products")]
    [Route("Catalog/GetProducts")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Json(products);
    }

    [HttpGet]
    [Route("Catalog/Details/{id?}")]
    [Route("product.html")]
    [Route("product")]
    [Route("product/{id?}")]
    public async Task<IActionResult> Details(int id)
    {
        var allProducts = await _productService.GetAllProductsAsync();
        if (id <= 0)
        {
            var first = allProducts.FirstOrDefault();
            if (first != null) id = first.Id;
        }

        ProductDto? product = allProducts.FirstOrDefault(x => x.Id == id);
        if (product == null)
        {
            product = await _productService.GetProductByIdAsync(id);
        }

        if (product == null)
        {
            return NotFound("Товар не найден");
        }

        var reviews = await _reviewService.GetReviewsForProductAsync(product.Id);

        var similar = allProducts
            .Where(p => p.Id != product.Id && (p.Brand == product.Brand || Math.Abs(p.Price - product.Price) <= 20000))
            .Take(4)
            .ToList();

        var vm = new ProductDetailsViewModel
        {
            Product = product,
            Reviews = reviews,
            SimilarProducts = similar
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(CreateReviewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Author))
        {
            model.Author = User.Identity?.Name ?? "Гость";
        }

        if (model.ProductId <= 0 || string.IsNullOrWhiteSpace(model.Text) || model.Rating < 1 || model.Rating > 5)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(new { success = false, message = "Некорректные данные отзыва" });
            }
            return RedirectToAction("Details", new { id = model.ProductId });
        }

        var req = new CreateReviewRequest(model.ProductId, model.Author.Trim(), model.Text.Trim(), model.Rating);
        var created = await _reviewService.AddReviewAsync(req);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Ok(new
            {
                success = true,
                review = new
                {
                    id = created.Id,
                    author = created.Author,
                    rating = created.Rating,
                    text = created.Text,
                    date = created.CreatedAt.ToString("dd.MM.yyyy")
                }
            });
        }

        return RedirectToAction("Details", new { id = model.ProductId });
    }

    [HttpPost]
    [HttpDelete]
    [Route("Catalog/DeleteReview/{id:int?}")]
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

public class CreateReviewModel
{
    public int ProductId { get; set; }
    public string Author { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public string Text { get; set; } = string.Empty;
}
