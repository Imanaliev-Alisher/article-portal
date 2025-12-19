using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ContentPortal.Data;
using ContentPortal.Models;
using Microsoft.AspNetCore.Identity;

namespace ContentPortal.Controllers;

[Authorize(Roles = "Admin,Editor")]
public class AdminController : Controller
{
    private readonly PortalContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(PortalContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Admin
    public IActionResult Index()
    {
        var stats = new
        {
            TotalArticles = _context.Articles.Count(),
            TotalCategories = _context.Categories.Count(),
            TotalUsers = _context.Users.Count(),
            RecentArticles = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.User)
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToList()
        };

        ViewBag.Stats = stats;
        return View();
    }

    // GET: Admin/Articles
    public IActionResult Articles(string? search)
    {
        var articlesQuery = _context.Articles
            .Include(a => a.Category)
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            articlesQuery = articlesQuery.Where(a => 
                a.Title.Contains(search) || a.Content.Contains(search));
            ViewData["CurrentFilter"] = search;
        }

        var articles = articlesQuery
            .OrderByDescending(a => a.PublishedAt)
            .ToList();

        return View(articles);
    }

    // GET: Admin/Categories
    public IActionResult Categories()
    {
        var categories = _context.Categories
            .Include(c => c.Articles)
            .OrderBy(c => c.Name)
            .ToList();

        return View(categories);
    }

    // GET: Admin/Users
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();

        var userRoles = new Dictionary<string, List<string>>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles.ToList();
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    // POST: Admin/DeleteArticle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteArticle(int id)
    {
        var article = _context.Articles.Find(id);
        if (article != null)
        {
            // Удаляем изображение если есть
            if (!string.IsNullOrEmpty(article.ImagePath))
            {
                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var filePath = Path.Combine(env.WebRootPath, article.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Articles.Remove(article);
            _context.SaveChanges();
            TempData["Success"] = "Статья удалена";
        }

        return RedirectToAction(nameof(Articles));
    }

    // POST: Admin/DeleteCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCategory(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["Success"] = "Категория удалена";
        }

        return RedirectToAction(nameof(Categories));
    }

    // GET: Admin/CreateCategory
    public IActionResult CreateCategory()
    {
        return View();
    }

    // POST: Admin/CreateCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateCategory(Category category)
    {
        if (!ModelState.IsValid)
            return View(category);

        _context.Categories.Add(category);
        _context.SaveChanges();
        TempData["Success"] = "Категория создана";

        return RedirectToAction(nameof(Categories));
    }

    // GET: Admin/EditCategory/5
    public IActionResult EditCategory(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: Admin/EditCategory/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditCategory(int id, Category category)
    {
        if (id != category.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(category);

        _context.Categories.Update(category);
        _context.SaveChanges();
        TempData["Success"] = "Категория обновлена";

        return RedirectToAction(nameof(Categories));
    }

    // GET: Admin/EditUser/id
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

        var model = new EditUserViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            CurrentRoles = userRoles.ToList(),
            AllRoles = allRoles!
        };

        return View(model);
    }

    // POST: Admin/EditUser/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
    {
        if (id != model.UserId)
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Обновляем информацию пользователя
        user.FullName = model.FullName;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // Обновляем роли
        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.SelectedRoles ?? new List<string>();

        // Удаляем роли, которые были сняты
        var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при удалении ролей");
                return View(model);
            }
        }

        // Добавляем новые роли
        var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Ошибка при добавлении ролей");
                return View(model);
            }
        }

        TempData["Success"] = "Пользователь обновлен";
        return RedirectToAction(nameof(Users));
    }
}
