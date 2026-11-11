using System.Security.Claims;
using EzShop.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

public class FavoritesController : Controller
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [Route("Favorites/GetMyFavorites")]
    [Route("api/favorites")]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, isAuth = false, ids = Array.Empty<int>() });
        }

        var ids = await _favoriteService.GetFavoriteProductIdsAsync(userId);
        return Ok(new { success = true, isAuth = true, ids });
    }

    [HttpPost]
    [Route("Favorites/Toggle/{productId?}")]
    [Route("api/favorites/toggle")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Toggle(int? productId, [FromBody] ToggleFavoriteRequest? req)
    {
        var id = productId ?? req?.ProductId ?? 0;
        if (id <= 0) return BadRequest(new { success = false, message = "Не указан ID товара" });

        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return Ok(new { success = true, isAuth = false, productId = id });
        }

        var isFavorite = await _favoriteService.ToggleFavoriteAsync(userId, id);
        return Ok(new { success = true, isAuth = true, productId = id, isFavorite });
    }

    [HttpPost]
    [Route("Favorites/SyncGuestFavorites")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SyncGuestFavorites([FromBody] List<int>? guestIds)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0 || guestIds == null || !guestIds.Any())
        {
            return Ok(new { success = true });
        }

        foreach (var id in guestIds.Distinct())
        {
            if (id > 0)
            {
                await _favoriteService.AddFavoriteAsync(userId, id);
            }
        }

        var allIds = await _favoriteService.GetFavoriteProductIdsAsync(userId);
        return Ok(new { success = true, ids = allIds });
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : 0;
    }
}

public class ToggleFavoriteRequest
{
    public int ProductId { get; set; }
}
