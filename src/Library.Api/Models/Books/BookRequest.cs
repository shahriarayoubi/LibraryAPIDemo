using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.Books;

public abstract class BookRequest
{
    [Required]
    [MaxLength(300)]
    public required string Title { get; set; }

    [Range(1, 9999)]
    public int YearOfFirstPublication { get; set; }

    [MaxLength(20)]
    public string? Isbn { get; set; }

    public string? Description { get; set; }

    [MaxLength(10)]
    public string? LanguageCode { get; set; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; set; }

    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }
}