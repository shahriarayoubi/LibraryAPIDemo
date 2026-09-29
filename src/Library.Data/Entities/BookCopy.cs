using Library.Data.Common;

namespace Library.Data.Entities;

public sealed class BookCopy : IAuditableEntity
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public Book Book { get; set; } = null!;

    public required string InventoryNumber { get; set; }

    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    public string? ShelfLocation { get; set; }

    public DateTime AcquiredUtc { get; set; }

    public ICollection<Loan> Loans { get; set; } = [];
    public DateTime CreatedUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime UpdatedUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

public enum BookCopyStatus
{
    Available = 1,
    OnLoan = 2,
    Reserved = 3,
    Lost = 4,
    Damaged = 5,
    Withdrawn = 6
}