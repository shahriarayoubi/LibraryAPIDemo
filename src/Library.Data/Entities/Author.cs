using Library.Data.Common;

namespace Library.Data.Entities;

public sealed class Author : IAuditableEntity
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public string? MiddleName { get; set; }

    public required string LastName { get; set; }

    public int? YearOfBirth { get; set; }

    public int? YearOfDeath { get; set; }

    public string? Country { get; set; }

    public string? Biography { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public ICollection<Book> Books { get; set; } = [];
}