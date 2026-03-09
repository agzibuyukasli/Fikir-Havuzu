using FikirHavuzu.Business.Abstract;
using FikirHavuzu.Business.Concrete;
using FikirHavuzu.DataAccess;
using FikirHavuzu.Entities;
using FikirHavuzu.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FikirHavuzu.WebUI.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly AppDbContext _context;


    public UserController(IUserService userService, AppDbContext context)
    {
        _userService = userService;
        _context = context;

    }

    private bool IsAuthorizedToManage()
    {
        return User.IsInRole("Admin");
    }

    [Authorize]
    public IActionResult Index()
    {
        var users = _userService.GetAll()
            .OrderByDescending(u => u.TotalScore)
            .ToList();

        var loggedInName = User.Identity?.Name;
        var currentUser = _context.Users.FirstOrDefault(u => u.Email == loggedInName);
        ViewBag.UserScore = currentUser?.TotalScore ?? 0;

        return View(users);
    }
    public IActionResult Create()
    {
        if (!IsAuthorizedToManage()) return Forbid();
        return View();
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        try
        {
            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("Password", "Kullanıcı için bir şifre belirlemelisiniz.");
                return View(user);
            }
            _userService.Add(user);
            TempData["Message"] = $"{user.FirstName} {user.LastName} sisteme başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Hata: " + ex.Message);
            return View(user);
        }
    }

    [HttpPost]
    public IActionResult UpdateAuthorities(UserAuthorityViewModel model)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        var currentAuthorities = _context.UserAuthorities.Where(ua => ua.UserId == model.UserId).ToList();
        _context.UserAuthorities.RemoveRange(currentAuthorities);
        _context.SaveChanges();

        if (model.Authorities != null)
        {
            foreach (var auth in model.Authorities.Where(x => x.IsSelected))
            {
                _context.UserAuthorities.Add(new UserAuthority
                {
                    UserId = model.UserId,
                    AuthorityId = auth.AuthorityId
                });
            }
            _context.SaveChanges();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ManageAuthorities(int id)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        var user = _userService.GetById(id);
        if (user == null) return NotFound();

        var allAuthorities = _context.Authorities.ToList();
        var userAuthorities = _context.UserAuthorities
            .Where(ua => ua.UserId == id)
            .Select(ua => ua.AuthorityId)
            .ToList();

        var viewModel = new UserAuthorityViewModel
        {
            UserId = id,
            UserName = $"{user.FirstName} {user.LastName}",
            Authorities = allAuthorities.Select(a => new AuthoritySelection
            {
                AuthorityId = a.Id,
                AuthorityName = a.Name,
                AuthorityCode = a.Code,
                IsSelected = userAuthorities.Contains(a.Id)
            }).ToList()
        };
        return View(viewModel);
    }
    public IActionResult Delete(int id)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        var user = _context.Users.Find(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
            TempData["Message"] = $"{user.FirstName} {user.LastName} başarıyla silindi.";
        }
        return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User updatedUser)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        var userInDb = await _context.Users.FindAsync(updatedUser.Id);
        if (userInDb == null) return NotFound();

        userInDb.FirstName = updatedUser.FirstName;
        userInDb.LastName = updatedUser.LastName;
        userInDb.Email = updatedUser.Email;
        userInDb.Role = updatedUser.Role;
        userInDb.RegistrationNumber = updatedUser.RegistrationNumber;
        userInDb.TCIdentityNumber = updatedUser.TCIdentityNumber;
        userInDb.PhoneNumber = updatedUser.PhoneNumber;

        try
        {
            _context.Users.Update(userInDb);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{userInDb.FirstName} {userInDb.LastName} bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Güncelleme sırasında bir hata oluştu: " + ex.Message;
            return View(updatedUser);
        }
    }

    public IActionResult Details(int id)
    {
        var user = _context.Users
            .Include(u => u.Ideas)
            .FirstOrDefault(u => u.Id == id);

        if (user == null) return NotFound();

        ViewBag.IdeaCount = user.Ideas.Count();

        var allUsers = _context.Users
            .OrderByDescending(u => u.TotalScore)
            .ToList();

        var userRank = allUsers.FindIndex(u => u.Id == id) + 1;

        ViewBag.GeneralRank = userRank;

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (!IsAuthorizedToManage()) return Forbid();

        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            TempData["Message"] = "Kullanıcı durumu güncellendi.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var loggedInName = User.Identity?.Name;

        if (string.IsNullOrEmpty(loggedInName))
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower().Trim() == loggedInName.ToLower().Trim());

        if (user == null)
        {
            return Content($"Hata: Giriş yapan isim '{loggedInName}' veritabanında bulunamadı.");
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(FikirHavuzu.Entities.User updatedUser)
    {
        var user = await _context.Users.FindAsync(updatedUser.Id);
        if (user == null) return NotFound();

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;

        await _context.SaveChangesAsync();
        TempData["Message"] = "Profil bilgileriniz başarıyla güncellendi.";

        return View(user);
    }

    [HttpGet]
    public IActionResult GetUserScore()
    {
        var userName = User.Identity.Name;

        if (string.IsNullOrEmpty(userName))
        {
            return Json(0);
        }

        var userScore = _context.Users
            .Where(u => u.Email == userName || u.FirstName + " " + u.LastName == userName)
            .Select(u => u.TotalScore)
            .FirstOrDefault();

        return Json(userScore);
    }

    [HttpGet]
    public IActionResult GetUserPurchasedItems()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return BadRequest();

        int userId = int.Parse(userIdClaim);

        var items = _context.PurchasedItems
            .Include(p => p.Product)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchaseDate)
            .Select(p => new
            {
                itemName = p.Product.Name, 
                price = p.Product.Price,
                date = p.PurchaseDate.ToString("dd.MM.yyyy")
            })
            .ToList();

        return Json(items);
    }

    [HttpGet]
    public IActionResult GetUserStats()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return BadRequest();
        int userId = int.Parse(userIdClaim);

        var totalIdeas = _context.Ideas.Count(i => i.UserId == userId);

        var approvedIdeas = _context.Ideas.Count(i => i.UserId == userId && i.IsApproved == true);

        var userScore = _context.Users.Where(u => u.Id == userId).Select(u => u.TotalScore).FirstOrDefault();

        double performanceRate = totalIdeas > 0 ? (double)approvedIdeas / totalIdeas * 100 : 0;

        return Json(new
        {
            totalIdeas = totalIdeas,
            approvedIdeas = approvedIdeas,
            userScore = userScore,
            performanceRate = Math.Round(performanceRate, 1)
        });
    }
}