using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FikirHavuzu.WebUI.Models;
using FikirHavuzu.Business.Abstract;
using FikirHavuzu.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FikirHavuzu.WebUI.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IIdeaService _ideaService;
    private readonly IUserService _userService;
    private readonly AppDbContext _context;

    public HomeController(IIdeaService ideaService, IUserService userService, AppDbContext context)
    {
        _ideaService = ideaService;
        _userService = userService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var leaderboard = _context.Users
            .OrderByDescending(u => u.TotalScore)
            .Take(5)
            .ToList();

        ViewBag.Leaderboard = leaderboard;

        var lastIdea = await _context.Ideas
            .OrderByDescending(x => x.CreatedDate) 
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();

        var userCount = await _context.Users.CountAsync();

        var viewModel = new DashboardViewModel
        {
            LastIdeaDate = lastIdea?.CreatedDate ?? DateTime.Now,
            LatestIdeaTitle = lastIdea?.Title ?? "Henüz fikir eklenmemiş",
            TotalUsers = userCount
        };

        var activeUser = await _context.Users.OrderByDescending(u => u.TotalScore).FirstOrDefaultAsync();
        ViewBag.MostActiveUser = activeUser != null ? (activeUser.FirstName + " " + activeUser.LastName) : "Yok";
        ViewBag.ActiveUserId = activeUser?.Id ?? 0;
        ViewBag.IdeaCount = await _context.Ideas.CountAsync();
        ViewBag.GeneralRank = 1;

        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
        ViewBag.UserScore = currentUser?.TotalScore ?? 0;

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Yardim()
    {
        return View();
    }

    public IActionResult Destek()
    {
        return View();
    }
}