using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AspireBlazor.Models;

public class Article
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string ContentHtml { get; set; } = "";

    [Required]
    public string AuthorId { get; set; } = "";

    [ForeignKey(nameof(AuthorId))]
    public IdentityUser? Author { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
