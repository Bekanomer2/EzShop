using Microsoft.AspNetCore.Identity;

namespace EzShop.Domain.Entities;

public class User : IdentityUser<int>
{
    public string? FullName { get; set; }
}
