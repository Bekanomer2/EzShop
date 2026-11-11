using EzShop.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EzShop.BLL.Services;

public static class AdminInitializer
{
    public static async Task SeedAdminAndRoleDataAsync(RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
    {
        // 1. Создаем роли "Admin" и "User" если отсутствуют
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (await roleManager.FindByNameAsync(role) is null)
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        // 2. Создаем администратора через UserManager с автоматическим хешированием пароля
        var adminEmail = "admin@ezshop.kg";
        var adminUser = await userManager.FindByEmailAsync(adminEmail) ?? await userManager.FindByNameAsync("admin");
        if (adminUser is null)
        {
            adminUser = new User
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Главный Администратор"
            };

            var result = await userManager.CreateAsync(adminUser, "admin");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Гарантируем, что у администратора установлен единственный пароль "admin"
            var isPassOk = await userManager.CheckPasswordAsync(adminUser, "admin");
            if (!isPassOk)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, "admin");
            }
        }

        // 3. Создаем тестового пользователя (клиента) для быстрого входа
        var testEmail = "beka@ezshop.kg";
        var testUser = await userManager.FindByEmailAsync(testEmail) ?? await userManager.FindByNameAsync("beka");
        if (testUser is null)
        {
            testUser = new User
            {
                UserName = "beka",
                Email = testEmail,
                EmailConfirmed = true,
                FullName = "Beka Test"
            };

            var result = await userManager.CreateAsync(testUser, "123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }
    }
}
