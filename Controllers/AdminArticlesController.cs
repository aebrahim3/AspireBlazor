using AspireBlazor.Data;
using AspireBlazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspireBlazor.Controllers;

// Handles admin CRUD operations for articles with role-based authorization.
[Authorize]
[Route("admin")]
public class AdminArticlesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminArticlesController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Fetch and display all articles sorted by most recent first.
    [HttpGet("")]
    [HttpGet("articles")]
    public async Task<IActionResult> Index()
    {
        List<Article> articles;
        if (User.IsInRole("Admin"))
        {
            articles = await _db.Articles.OrderByDescending(a => a.CreatedAtUtc).ToListAsync();
        }
        else if (User.IsInRole("Writer"))
        {
            var userId = _userManager.GetUserId(User);
            articles = await _db.Articles
                .Where(a => a.AuthorId == userId)
                .OrderByDescending(a => a.CreatedAtUtc)
                .ToListAsync();
            return View(articles);
        }
        else
        {
            return Forbid();
        }

        return View(articles);

    }

    // Display the article creation form.
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    // Create a new article and save to database.
    [HttpPost("create")]
    public async Task<IActionResult> Create(Article article)
    {
        // AuthorId and Author are set server-side, so clear their validation errors.
        ModelState.Remove("AuthorId");
        ModelState.Remove("Author");
        ModelState.Remove("Author.UserName");

        // Return form if validation fails.
        if (!ModelState.IsValid)
            return View(article);

        article.Id = 0;
        article.CreatedAtUtc = DateTime.UtcNow;
        article.UpdatedAtUtc = DateTime.UtcNow;
        article.AuthorId = _userManager.GetUserId(User) ?? "";

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Display the article edit form.
    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await _db.Articles.FindAsync(id);
        // Return 404 if article doesn't exist.
        if (article is null)
            return NotFound();

        return View(article);
    }

    // Update an existing article with new data.
    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, Article article)
    {
        // Verify the ID matches the article being updated.
        if (id != article.Id)
            return BadRequest();

        // AuthorId and Author are not editable, so clear their validation errors.
        ModelState.Remove("AuthorId");
        ModelState.Remove("Author");
        ModelState.Remove("Author.UserName");

        // Return form if validation fails.
        if (!ModelState.IsValid)
            return View(article);

        var existing = await _db.Articles.FindAsync(id);
        // Return 404 if article doesn't exist.
        if (existing is null)
            return NotFound();

        existing.Title = article.Title;
        existing.ContentHtml = article.ContentHtml;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _db.Articles.Update(existing);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Delete an article from the database.
    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _db.Articles.FindAsync(id);
        // Return 404 if article doesn't exist.
        if (article is null)
            return NotFound();

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
