using System.Security.Claims;
using FikirHavuzu.DataAccess;
using FikirHavuzu.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FikirHavuzu.WebUI.Controllers;

[Authorize]
public class MarketController : Controller
{
    private readonly AppDbContext _context;

    public MarketController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(userIdClaim);
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.UserScore = user.TotalScore;
        var products = _context.Products.ToList();

        return View(products);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Ürün markete başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [HttpPost]
    public IActionResult Buy(int productId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return BadRequest("Oturum bulunamadı.");

        int userId = int.Parse(userIdClaim);
        var user = _context.Users.Find(userId);
        var product = _context.Products.Find(productId);

        if (product == null || user == null) return NotFound("Kullanıcı veya ürün bulunamadı.");

        if (product.Stock <= 0)
        {
            TempData["Error"] = "Bu ürünün stoğu tükenmiş!";
            return RedirectToAction("Index");
        }

        if (user.TotalScore < product.Price)
        {
            TempData["Error"] = "Puanınız bu ürün için yetersiz!";
            return RedirectToAction("Index");
        }

        user.TotalScore -= product.Price;
        product.Stock -= 1;            

        var purchase = new PurchasedItem
        {
            UserId = userId,
            ProductId = productId,
            PurchaseDate = DateTime.Now
        };
        _context.PurchasedItems.Add(purchase);

        _context.SaveChanges();

        TempData["Message"] = $"{product.Name} başarıyla alındı! Envanterinize eklendi.";

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Ürün başarıyla güncellendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Ürün marketten kaldırıldı.";
        }
        return RedirectToAction(nameof(Index));
    }
}