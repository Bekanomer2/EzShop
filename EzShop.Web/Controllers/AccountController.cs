using EzShop.Domain.Entities;
using EzShop.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EzShop.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Account/Login")]
    [Route("Login")]
    [Route("login.html")]
    public IActionResult Login(string? returnUrl = null, string? tab = "login")
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Catalog");
        }

        ViewBag.ActiveTab = string.Equals(tab, "register", StringComparison.OrdinalIgnoreCase) ? "register" : "login";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("Account/Login")]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ActiveTab = "login";
            return View(model);
        }

        var login = model.Login.Trim();
        var password = model.Password.Trim();

        var user = await _userManager.FindByNameAsync(login) ?? await _userManager.FindByEmailAsync(login);
        if (user != null)
        {
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                TempData["JustLoggedIn"] = true;
                TempData["SuccessMessage"] = $"С возвращением, {user.UserName}!";
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || string.Equals(user.UserName, "admin", StringComparison.OrdinalIgnoreCase);
                if (isAdmin && (string.IsNullOrEmpty(model.ReturnUrl) || model.ReturnUrl == "/"))
                {
                    return Redirect("/admin.html");
                }
                return RedirectAfterLogin(model.ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Неверный пароль. Попробуйте ещё раз.");
        }
        else
        {
            // Если аккаунт admin ещё не существовал (например база пуста), создаем на лету с паролем "admin"
            if (string.Equals(login, "admin", StringComparison.OrdinalIgnoreCase) || string.Equals(login, "admin@ezshop.kg", StringComparison.OrdinalIgnoreCase))
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin@ezshop.kg",
                    EmailConfirmed = true,
                    FullName = "Главный Администратор"
                };
                var adminPass = "admin";
                var createRes = await _userManager.CreateAsync(admin, adminPass);
                if (createRes.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                    await _signInManager.SignInAsync(admin, isPersistent: model.RememberMe);
                    TempData["JustLoggedIn"] = true;
                    TempData["SuccessMessage"] = "Добро пожаловать, Главный Администратор!";
                    return Redirect("/admin.html");
                }
            }

            ModelState.AddModelError(string.Empty, $"Пользователь «{login}» не найден в базе данных. Перейдите на вкладку «Регистрация» и создайте аккаунт.");
        }

        ViewBag.ActiveTab = "login";
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Account/Register")]
    [Route("Register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Catalog");
        }

        return RedirectToAction("Login", new { tab = "register" });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("Account/Register")]
    [Route("Register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "Пожалуйста, заполните все обязательные поля (Логин, Email, Пароль).");
            ViewBag.ActiveTab = "register";
            return View("Login", new LoginViewModel());
        }

        var username = model.Username.Trim();
        var email = model.Email.Trim();
        var password = model.Password.Trim();

        // 1. Проверка формата Email
        try
        {
            var m = new System.Net.Mail.MailAddress(email);
            if (m.Address != email) throw new FormatException();
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Укажите корректный адрес электронной почты (например, name@example.com).");
            ViewBag.ActiveTab = "register";
            ViewBag.RegUsername = username;
            ViewBag.RegEmail = email;
            return View("Login", new LoginViewModel());
        }

        // 2. Проверка уникальности логина
        var userByName = await _userManager.FindByNameAsync(username);
        if (userByName != null)
        {
            ModelState.AddModelError(string.Empty, "Пользователь с таким логином уже существует.");
            ViewBag.ActiveTab = "register";
            ViewBag.RegUsername = username;
            ViewBag.RegEmail = email;
            return View("Login", new LoginViewModel());
        }

        // 3. Проверка уникальности почты (UserManager + регистронезависимо в БД)
        var userByEmail = await _userManager.FindByEmailAsync(email)
            ?? await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                _userManager.Users, u => u.Email != null && u.Email.ToLower() == email.ToLower());

        if (userByEmail != null)
        {
            ModelState.AddModelError(string.Empty, "Аккаунт с такой почтой уже зарегистрирован. Пожалуйста, войдите или укажите другой Email.");
            ViewBag.ActiveTab = "register";
            ViewBag.RegUsername = username;
            ViewBag.RegEmail = email;
            return View("Login", new LoginViewModel());
        }

        var user = new User
        {
            UserName = username,
            Email = email,
            FullName = string.IsNullOrWhiteSpace(model.FullName) ? username : model.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                if (err.Code.Contains("DuplicateEmail", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Аккаунт с такой почтой уже зарегистрирован.");
                }
                else if (err.Code.Contains("DuplicateUserName", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким логином уже существует.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
            }
            ViewBag.ActiveTab = "register";
            ViewBag.RegUsername = username;
            ViewBag.RegEmail = email;
            return View("Login", new LoginViewModel());
        }

        await _userManager.AddToRoleAsync(user, "User");
        await _signInManager.SignInAsync(user, isPersistent: true);

        TempData["JustLoggedIn"] = true;
        TempData["SuccessMessage"] = "Регистрация прошла успешно! Добро пожаловать в EzShop.";
        return RedirectToAction("Index", "Catalog");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "Вы успешно вышли из учетной записи.";
        return RedirectToAction("Index", "Catalog");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [Route("Account/CurrentUser")]
    [Route("Account/Me")]
    [Route("api/auth/me")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name))
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var isAdmin = User.IsInRole("Admin") || string.Equals(User.Identity.Name, "admin", StringComparison.OrdinalIgnoreCase);

            return Ok(new
            {
                isAuthenticated = true,
                user = new
                {
                    id = user?.Id.ToString() ?? "0",
                    username = User.Identity.Name,
                    email = user?.Email ?? "",
                    role = isAdmin ? "admin" : "user"
                }
            });
        }

        return Ok(new
        {
            isAuthenticated = false,
            user = (object?)null
        });
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/Account/Login")
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Catalog");
    }
}
