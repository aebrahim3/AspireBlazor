using CmsBackend.Data;
using CmsBackend.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CmsBackend.Controllers;

// Provides REST API endpoints for article CRUD operations with HTML sanitization.
[ApiController]
[Route("api/articles")]
public class ArticlesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public ArticlesApiController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Retrieve all articles sorted by most recent first.
    [HttpGet]
    public async Task<List<Article>> GetAll()
        => await _db.Articles.OrderByDescending(a => a.CreatedAtUtc).ToListAsync();

    // Retrieve a single article by ID.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Article>> GetById(int id)
    {
        var a = await _db.Articles.FindAsync(id);
        // Return 404 if article doesn't exist.
        return a is null ? NotFound() : Ok(a);
    }

    // Create new article and sanitize HTML content before saving.
    [HttpPost]
    public async Task<ActionResult<Article>> Create([FromBody] Article input)
    {
        // Validate that the AuthorId corresponds to an existing user.
        var author = await _userManager.FindByIdAsync(input.AuthorId);
        if (author is null)
            return BadRequest(new { error = "The specified AuthorId does not match any existing user." });

        input.Id = 0;
        // Sanitize HTML to prevent XSS attacks.
        input.ContentHtml = _sanitizer.Sanitize(input.ContentHtml);
        input.CreatedAtUtc = DateTime.UtcNow;
        input.UpdatedAtUtc = DateTime.UtcNow;

        _db.Articles.Add(input);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    // Update an existing article with sanitized HTML content.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Article input)
    {
        var a = await _db.Articles.FindAsync(id);
        // Return 404 if article doesn't exist.
        if (a is null) return NotFound();

        // Validate that the AuthorId corresponds to an existing user.
        var author = await _userManager.FindByIdAsync(input.AuthorId);
        if (author is null)
            return BadRequest(new { error = "The specified AuthorId does not match any existing user." });

        a.Title = input.Title;
        // Sanitize HTML to prevent XSS attacks.
        a.ContentHtml = _sanitizer.Sanitize(input.ContentHtml);
        a.AuthorId = input.AuthorId;
        a.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Delete an article by ID.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Articles.FindAsync(id);
        // Return 404 if article doesn't exist.
        if (a is null) return NotFound();

        _db.Articles.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
