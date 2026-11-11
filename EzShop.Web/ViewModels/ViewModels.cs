using System.ComponentModel.DataAnnotations;
using EzShop.BLL.DTO;

namespace EzShop.Web.ViewModels;

public class CatalogIndexViewModel
{
    public IReadOnlyList<ProductDto> Products { get; set; } = new List<ProductDto>();
    public IReadOnlyList<string> Brands { get; set; } = new List<string>();
    public string? SelectedBrand { get; set; }
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; }
}

public class ProductDetailsViewModel
{
    public ProductDto Product { get; set; } = null!;
    public IReadOnlyList<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    public IReadOnlyList<ProductDto> SimilarProducts { get; set; } = new List<ProductDto>();
    public CreateReviewRequest NewReview { get; set; } = null!;
}

public class OrdersIndexViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public IReadOnlyList<OrderDto> Orders { get; set; } = new List<OrderDto>();
}

public class AdminDashboardViewModel
{
    public IReadOnlyList<ProductDto> Products { get; set; } = new List<ProductDto>();
    public IReadOnlyList<OrderDto> Orders { get; set; } = new List<OrderDto>();
    public decimal TotalRevenue { get; set; }
    public int ActiveOrdersCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class AdminOrdersViewModel
{
    public IReadOnlyList<OrderDto> Orders { get; set; } = new List<OrderDto>();
    public int TotalCount { get; set; }
    public decimal Revenue { get; set; }
    public int InProgressCount { get; set; }
    public int ReturnsCount { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя или Email")]
    [Display(Name = "Логин или Email")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = true;
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "ФИО (по желанию)")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум {2} символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    public string? ConfirmPassword { get; set; }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string ErrorMessage { get; set; } = "Произошла непредвиденная ошибка при обработке запроса.";
}
