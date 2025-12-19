using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ContentPortal.Data;
using ContentPortal.Models;
using System.Security.Claims;

namespace ContentPortal.Controllers;

public class ArticlesController : Controller
{
    private readonly PortalContext _context;
    private readonly IWebHostEnvironment _environment;

    public ArticlesController(PortalContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: Articles
    public IActionResult Index(string? search, int? categoryId)
    {
        var articlesQuery = _context.Articles
            .Include(a => a.Category)
            .Include(a => a.User)
            .AsQueryable();

        // Поиск
        if (!string.IsNullOrWhiteSpace(search))
        {
            articlesQuery = articlesQuery.Where(a => 
                a.Title.Contains(search) || a.Content.Contains(search));
            ViewData["CurrentFilter"] = search;
        }

        // Фильтр по категории
        if (categoryId.HasValue)
        {
            articlesQuery = articlesQuery.Where(a => a.CategoryId == categoryId.Value);
            ViewData["CurrentCategory"] = categoryId.Value;
        }

        var articles = articlesQuery.OrderByDescending(a => a.PublishedAt).ToList();
        
        // Список категорий для фильтра
        ViewBag.Categories = _context.Categories.ToList();
        
        return View(articles);
    }

    // GET: Articles/Details/5
    public IActionResult Details(int id)
    {
        var article = _context.Articles
            .Include(a => a.Category)
            .Include(a => a.User)
            .FirstOrDefault(a => a.Id == id);
            
        if (article == null) 
            return NotFound();
            
        return View(article);
    }

    // GET: Articles/Create
    [Authorize]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    // POST: Articles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(Article article, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", article.CategoryId);
            return View(article);
        }

        // Загрузка изображения
        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            
            article.ImagePath = "/uploads/" + fileName;
        }

        // Получаем ID текущего пользователя
        article.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        article.PublishedAt = DateTime.UtcNow;

        _context.Articles.Add(article);
        _context.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }

    // GET: Articles/Edit/5
    [Authorize]
    public IActionResult Edit(int id)
    {
        var article = _context.Articles.Find(id);
        
        if (article == null)
            return NotFound();

        // Проверка прав - только автор может редактировать
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (article.UserId != userId)
            return Forbid();

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", article.CategoryId);
        return View(article);
    }

    // POST: Articles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, Article article, IFormFile? imageFile)
    {
        if (id != article.Id)
            return NotFound();

        var existingArticle = _context.Articles.AsNoTracking().FirstOrDefault(a => a.Id == id);
        
        if (existingArticle == null)
            return NotFound();

        // Проверка прав
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existingArticle.UserId != userId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", article.CategoryId);
            return View(article);
        }

        // Загрузка нового изображения
        if (imageFile != null && imageFile.Length > 0)
        {
            // Удаляем старое изображение
            if (!string.IsNullOrEmpty(existingArticle.ImagePath))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, existingArticle.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            
            article.ImagePath = "/uploads/" + fileName;
        }
        else
        {
            article.ImagePath = existingArticle.ImagePath;
        }

        article.UserId = existingArticle.UserId;
        article.PublishedAt = existingArticle.PublishedAt;

        _context.Articles.Update(article);
        _context.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }

    // GET: Articles/Delete/5
    [Authorize]
    public IActionResult Delete(int id)
    {
        var article = _context.Articles
            .Include(a => a.Category)
            .Include(a => a.User)
            .FirstOrDefault(a => a.Id == id);
            
        if (article == null)
            return NotFound();

        // Проверка прав
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (article.UserId != userId)
            return Forbid();

        return View(article);
    }

    // POST: Articles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult DeleteConfirmed(int id)
    {
        var article = _context.Articles.Find(id);
        
        if (article == null)
            return NotFound();

        // Проверка прав
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (article.UserId != userId)
            return Forbid();

        // Удаляем изображение
        if (!string.IsNullOrEmpty(article.ImagePath))
        {
            var filePath = Path.Combine(_environment.WebRootPath, article.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _context.Articles.Remove(article);
        _context.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }
}
