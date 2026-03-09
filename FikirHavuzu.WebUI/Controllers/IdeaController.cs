using FikirHavuzu.Business.Abstract;
using FikirHavuzu.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FikirHavuzu.DataAccess;
using ClosedXML.Excel;
using System.IO;

namespace FikirHavuzu.WebUI.Controllers;

[Authorize]
public class IdeaController : Controller
{
    private readonly IIdeaService _ideaService;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _context;

    public IdeaController(IIdeaService ideaService, IUserService userService, IWebHostEnvironment env, AppDbContext context)
    {
        _ideaService = ideaService;
        _userService = userService;
        _env = env;
        _context = context;
    }

    public IActionResult Index(string searchTitle, string searchSubject, DateTime? searchDate, string searchUser)
    {

        var leaderboard = _context.Users
        .OrderByDescending(u => u.TotalScore)
        .Take(5)
        .ToList();

        ViewBag.Leaderboard = leaderboard;

        var userIdClaim = User.FindFirst("UserId")?.Value;
        ViewBag.CurrentUserId = string.IsNullOrEmpty(userIdClaim) ? 0 : int.Parse(userIdClaim);

        IQueryable<FikirHavuzu.Entities.Idea> ideasQuery = _context.Ideas
            .Include(x => x.User)
            .Include(x => x.Evaluations);

        if (!string.IsNullOrEmpty(searchTitle))
            ideasQuery = ideasQuery.Where(i => i.Title.Contains(searchTitle));

        if (!string.IsNullOrEmpty(searchSubject))
            ideasQuery = ideasQuery.Where(i => i.Subject.Contains(searchSubject));

        if (searchDate.HasValue)
            ideasQuery = ideasQuery.Where(i => i.CreatedDate.Date == searchDate.Value.Date);

        if (!string.IsNullOrEmpty(searchUser))
            ideasQuery = ideasQuery.Where(i => (i.User.FirstName + " " + i.User.LastName).Contains(searchUser));


        var result = ideasQuery
            .OrderByDescending(i => i.CreatedDate)
            .ToList();

        return View(result);
    }

    [HttpGet]
    public IActionResult Evaluate(int id)
    {
        var idea = _context.Ideas.Include(u => u.User).FirstOrDefault(x => x.Id == id);
        if (idea == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirst("UserId").Value);

        if (idea.UserId == currentUserId)
        {
            TempData["Error"] = "Kendi fikrinize puan veremezsiniz!";
            return RedirectToAction("Index");
        }

        var existingEval = _context.IdeaEvaluations
            .FirstOrDefault(e => e.IdeaId == id && e.EvaluatorUserId == currentUserId);

        if (existingEval != null)
        {
            existingEval.Idea = idea;
            return View(existingEval);
        }

        var newModel = new IdeaEvaluation { IdeaId = id, Idea = idea };
        return View(newModel);
    }

    [HttpPost]
    public IActionResult Evaluate(IdeaEvaluation model)
    {
        var currentUserId = int.Parse(User.FindFirst("UserId").Value);

        var existingEval = _context.IdeaEvaluations
            .FirstOrDefault(e => e.IdeaId == model.IdeaId && e.EvaluatorUserId == currentUserId);

        var idea = _context.Ideas.Find(model.IdeaId);
        var owner = _context.Users.Find(idea.UserId);

        if (existingEval != null)
        {
            if (owner != null)
            {
                owner.TotalScore = (owner.TotalScore - existingEval.Score) + model.Score;
            }

            existingEval.Score = model.Score;
            existingEval.Status = model.Status;
            existingEval.Description = model.Description;
            existingEval.EvaluationDate = DateTime.Now;
        }
        else
        {
            model.Id = 0;
            model.EvaluatorUserId = currentUserId;
            model.EvaluationDate = DateTime.Now;

            model.Idea = null;
            model.EvaluatorUser = null;

            _context.IdeaEvaluations.Add(model);

            if (owner != null)
            {
                owner.TotalScore += model.Score;
            }
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Create()
    {
        ViewBag.Users = new SelectList(_userService.GetAll(), "Id", "FirstName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Idea idea, IFormFile document)
    {
        var loggedInUserEmail = User.Identity?.Name;


        if (string.IsNullOrEmpty(loggedInUserEmail))
        {
            return Content("Hata: Oturum isminiz boş geldi. Lütfen çıkış yapıp tekrar giriş yapın.");
        }


        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loggedInUserEmail)
            ?? await _context.Users.FirstOrDefaultAsync(u => (u.FirstName + " " + u.LastName) == loggedInUserEmail);

        if (user == null)
        {
            return Content($"Hata: '{loggedInUserEmail}' ismiyle veritabanında eşleşen kullanıcı yok.");
        }

        idea.UserId = user.Id;
        idea.CreatedDate = DateTime.Now;

        if (document != null && document.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(document.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(fileStream);
            }

            idea.DocumentPath = "/uploads/documents/" + fileName;
        }

        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("CreatedDate");
        ModelState.Remove("document"); 

        if (ModelState.IsValid)
        {
            _context.Add(idea);

            user.TotalScore += 10;
            _context.Update(user);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🎉 Fikriniz başarıyla eklendi! +10 Fikir Puanı kazandınız.";

            return RedirectToAction(nameof(Index));
        }

        var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        TempData["ErrorMessage"] = "Bir sorun oluştu: " + errors;

        return View(idea);
    }

    public IActionResult ExportToExcel()
    {
        var ideas = _context.Ideas.Include(i => i.User).ToList();
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Fikirler");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Fikir Başlığı";
            worksheet.Cell(currentRow, 2).Value = "Konu";
            worksheet.Cell(currentRow, 3).Value = "Ekleyen";
            worksheet.Cell(currentRow, 4).Value = "Puan";
            worksheet.Cell(currentRow, 5).Value = "Durum";

            foreach (var idea in ideas)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = idea.Title;
                worksheet.Cell(currentRow, 2).Value = idea.Subject;
                worksheet.Cell(currentRow, 3).Value = idea.User?.FirstName + " " + idea.User?.LastName;
                worksheet.Cell(currentRow, 4).Value = idea.Score;
                worksheet.Cell(currentRow, 5).Value = idea.Status;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FikirRaporu.xlsx");
            }
        }
    }

    [Authorize]
    public async Task<IActionResult> DegerlendirmeDetay(int id)
    {
        var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == id);
        if (idea == null) return NotFound();

        var evaluations = await _context.IdeaEvaluations
            .Where(e => e.IdeaId == id)
            .ToListAsync();

        var evalList = new List<dynamic>();

        foreach (var e in evaluations)
        {
            var evaluator = _context.Users.FirstOrDefault(u => u.Id == e.EvaluatorUserId);

            evalList.Add(new
            {
                Score = e.Score,
                Description = e.Description,
                Status = e.Status,
                UserName = evaluator != null ? $"{evaluator.FirstName} {evaluator.LastName}" : "Bilinmeyen Kullanıcı",
                Date = e.EvaluationDate
            });
        }

        ViewBag.Evaluations = evalList;
        return View(idea);
    }

    [HttpGet]
    public IActionResult GetUserIdeas()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Content("Oturum hatası.");

        var userId = int.Parse(userIdClaim);
        var user = _context.Users.Find(userId);

        var loggedInEmail = User.Identity?.Name;

        var ideas = _context.Ideas
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedDate)
            .ToList();

        string html = $@"
        <div class='text-center mb-4'>
            <div class='display-1 text-primary'><i class='bi bi-person-circle'></i></div>
            <h3 class='fw-bold mb-0 text-dark'>{user?.FirstName} {user?.LastName}</h3>
            <div class='mt-2'>
                <span class='badge bg-warning text-dark fs-6 shadow-sm px-3 rounded-pill'>
                    <i class='bi bi-star-fill me-1'></i> {user?.TotalScore ?? 0} Puan
                </span>
                <span class='badge bg-info text-dark fs-6 shadow-sm ms-2 px-3 rounded-pill'>
                    <i class='bi bi-trophy-fill me-1'></i> {user?.Rank ?? "Yeni Fikirci"}
                </span>
            </div>
            <hr class='opacity-10' />
        </div>
        <h5 class='fw-bold mb-3 text-primary'><i class='bi bi-lightbulb-fill text-warning me-2'></i> Paylaşımlarım</h5>";

        if (!ideas.Any())
        {
            html += "<p class='text-muted text-center py-4'>Henüz bir fikir paylaşmadınız.</p>";
        }
        else
        {
            html += "<div class='list-group border-0 gap-3'>";
            foreach (var idea in ideas)
            {
                var evalCount = _context.IdeaEvaluations.Count(e => e.IdeaId == idea.Id);
                string statusColor = idea.Status == "Onaylandı" ? "success" : "warning";

                html += $@"
                <div class='list-group-item p-3 rounded-4 shadow-sm border-0 border-start border-4 border-primary bg-white'>
                    <div class='d-flex justify-content-between align-items-start mb-2'>
                        <div>
                            <h6 class='mb-0 fw-bold text-dark fs-5'>{idea.Title}</h6>
                            <small class='text-muted'><i class='bi bi-calendar3 me-1'></i>{idea.CreatedDate.ToShortDateString()}</small>
                        </div>
                        <span class='badge rounded-pill bg-{statusColor}-subtle text-{statusColor} border border-{statusColor} px-3 py-2'>
                            {idea.Status}
                        </span>
                    </div>
                    
                    <hr class='opacity-5 my-2'>

                    <div class='d-flex justify-content-between align-items-center mt-3'>
                        <div class='d-flex align-items-center gap-2'>
                            <span class='badge bg-light text-primary border rounded-pill px-3 py-2' style='font-size: 0.75rem;'>
                                <i class='bi bi-people-fill me-1'></i> {evalCount} Değerlendirme
                            </span>";

                if (User.IsInRole("Admin") || loggedInEmail == "admin@fikirhavuzu.com")
                {
                    html += $@"
                    <button type='button' onclick='deleteIdea({idea.Id})' class='btn btn-sm btn-outline-danger rounded-pill border-0 px-2' title='Fikri Sil'>
                        <i class='bi bi-trash3-fill'></i>
                    </button>";
                }

                html += $@"
                        </div>
                        <a href='/Idea/DegerlendirmeDetay/{idea.Id}' class='btn btn-sm btn-primary rounded-pill px-4 fw-bold shadow-sm transition-all'>
                            <i class='bi bi-eye-fill me-1'></i> Detaylar
                        </a>
                    </div>
                </div>";
            }
            html += "</div>";
        }
        return Content(html);
    }
    [HttpGet]
    public IActionResult GetUserStats()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return BadRequest();
        int userId = int.Parse(userIdClaim);

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        var ideas = _context.Ideas.Where(i => i.UserId == userId).ToList();

        int total = ideas.Count;
        int approved = ideas.Count(i => i.IsApproved == true);

        double rate = total > 0 ? Math.Round((double)approved / total * 100, 1) : 0;

        return Json(new
        {
            total = total,   
            approved = approved,
            score = user.TotalScore,
            rate = rate  
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteIdea(int id)
    {
        var idea = await _context.Ideas.FindAsync(id);

        if (idea == null) return NotFound();

        var evaluations = _context.IdeaEvaluations.Where(e => e.IdeaId == id);
        _context.IdeaEvaluations.RemoveRange(evaluations);

        _context.Ideas.Remove(idea);
        await _context.SaveChangesAsync();

        return Ok();
    }
}