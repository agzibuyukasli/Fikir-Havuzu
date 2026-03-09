using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using FikirHavuzu.Business.Abstract;
using System.Security.Claims;
using FikirHavuzu.Entities;
using FikirHavuzu.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FikirHavuzu.WebUI.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly AppDbContext _context;

    public AccountController(IUserService userService, AppDbContext context)
    {
        _userService = userService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

        if (user != null)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Idea");
        }

        ModelState.AddModelError("", "Email veya şifre hatalı!");
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(User user)
    {
        if (user != null)
        {
            user.IsActive = true;
            _userService.Add(user);
            return RedirectToAction("Login");
        }

        ViewBag.Error = "Kayıt sırasında bir hata oluştu.";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login","Account");
    }
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult AdminLogin() => View();

    [HttpPost]
    public async Task<IActionResult> AdminLogin(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.Role == "Admin");

        if (user != null)
        {

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("FullName", $"{user.FirstName} {user.LastName}")
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Geçersiz admin bilgileri veya yetkisiz erişim!";
        return View();
    }
}