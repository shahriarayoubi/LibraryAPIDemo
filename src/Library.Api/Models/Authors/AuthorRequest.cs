using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.Authors;

public abstract class AuthorRequest : IValidatableObject
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    public int? YearOfBirth { get; set; }

    public int? YearOfDeath { get; set; }

    public string? Country { get; set; }

    public string? Biography { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (YearOfBirth.HasValue &&
            YearOfDeath.HasValue &&
            YearOfDeath < YearOfBirth)
        {
            yield return new ValidationResult(
                "Year of death cannot be earlier than year of birth.",
                [nameof(YearOfDeath)]);
        }
    }
}