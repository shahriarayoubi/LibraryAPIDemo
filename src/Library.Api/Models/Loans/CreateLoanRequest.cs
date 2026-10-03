using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.Loans;

public sealed class CreateLoanRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int BookCopyId { get; set; }

    [Range(1, int.MaxValue)]
    public int BorrowerId { get; set; }

    public DateTime LoanedUtc { get; set; }

    public DateTime DueUtc { get; set; }

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
    }
}