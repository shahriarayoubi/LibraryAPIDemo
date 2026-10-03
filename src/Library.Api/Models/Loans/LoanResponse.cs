namespace Library.Api.Models.Loans;

public sealed class LoanResponse
{
    public int Id { get; set; }

    public int BookCopyId { get; set; }

    public int BorrowerId { get; set; }

    public DateTime LoanedUtc { get; set; }

    public DateTime DueUtc { get; set; }

    public DateTime? ReturnedUtc { get; set; }

    public DateTime? RenewedUtc { get; set; }

    public int RenewalCount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
