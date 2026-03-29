using System.ComponentModel.DataAnnotations;

namespace CmsBackend.Models;

public class Article
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string ContentHtml { get; set; } = "";

    [MaxLength(100)]
    public string AuthorName { get; set; } = "admin";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
