using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.Loans;

public abstract class LoanRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int BookCopyId { get; set; }

    [Range(1, int.MaxValue)]
    public int BorrowerId { get; set; }

    public DateTime LoanedUtc { get; set; }

    public DateTime DueUtc { get; set; }

    public DateTime? ReturnedUtc { get; set; }

    public DateTime? RenewedUtc { get; set; }

    [Range(0, int.MaxValue)]
    public int RenewalCount { get; set; }

    [MaxLength(1_000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (DueUtc <= LoanedUtc)
        {
            yield return new ValidationResult(
                "Due date must be after the loaned date.",
                [nameof(DueUtc)]);
        }

        if (ReturnedUtc.HasValue && ReturnedUtc < LoanedUtc)
        {
            yield return new ValidationResult(
                "Returned date cannot be earlier than the loaned date.",
                [nameof(ReturnedUtc)]);
        }
    }
}
