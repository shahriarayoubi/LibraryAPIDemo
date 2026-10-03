namespace Library.Api.Models.Me;

public sealed class MyLoanResponse
{
    public int LoanId { get; set; }

    public int BookCopyId { get; set; }

    public required string BookTitle { get; set; }

    public DateTime LoanedUtc { get; set; }

    public DateTime DueUtc { get; set; }

    public DateTime? ReturnedUtc { get; set; }

    public int RenewalCount { get; set; }
}