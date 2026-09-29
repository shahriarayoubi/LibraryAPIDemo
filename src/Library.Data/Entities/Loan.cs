using Library.Data.Common;

namespace Library.Data.Entities;

public sealed class Loan : IAuditableEntity
{
    public int Id { get; set; }

    public int BookCopyId { get; set; }

    public BookCopy BookCopy { get; set; } = null!;

    public int BorrowerId { get; set; }

    public Borrower Borrower { get; set; } = null!;

    public DateTime LoanedUtc { get; set; }

    public DateTime DueUtc { get; set; }

    public DateTime? ReturnedUtc { get; set; }

    public DateTime? RenewedUtc { get; set; }

    public int RenewalCount { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime UpdatedUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}