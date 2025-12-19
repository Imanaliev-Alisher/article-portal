using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContentPortal.Data;
using ContentPortal.Models;

namespace ContentPortal.Controllers;

public class CategoriesController : Controller
{
    private readonly PortalContext _context;

    public CategoriesController(PortalContext context)
    {
        _context = context;
    }

    // GET: Categories
    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    // GET: Categories/Details/5
    public IActionResult Details(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // GET: Categories/Create
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Create(Category category)
    {
        if (!ModelState.IsValid)
            return View(category);

        _context.Categories.Add(category);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    [Authorize]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Edit(int id, Category category)
    {
        if (id != category.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(category);

        _context.Categories.Update(category);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    [Authorize]
    public IActionResult Delete(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult DeleteConfirmed(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
}
