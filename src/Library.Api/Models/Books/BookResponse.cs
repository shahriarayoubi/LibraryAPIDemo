namespace Library.Api.Models.Books;

public sealed class BookResponse
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int YearOfFirstPublication { get; set; }

    public string? Isbn { get; set; }

    public string? Description { get; set; }

    public string? LanguageCode { get; set; }

    public int? PageCount { get; set; }

    public int AuthorId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}