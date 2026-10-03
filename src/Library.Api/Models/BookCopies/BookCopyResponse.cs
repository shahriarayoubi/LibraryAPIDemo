using Library.Data.Entities;

namespace Library.Api.Models.BookCopies;

public sealed class BookCopyResponse
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public required string InventoryNumber { get; set; }

    public BookCopyStatus Status { get; set; }

    public string? ShelfLocation { get; set; }

    public DateTime AcquiredUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
