namespace Library.Api.Models.Borrowers;

public sealed class BorrowerResponse
{
    public int Id { get; set; }

    public required string MembershipNumber { get; set; }

    public required string FirstName { get; set; }

    public string? MiddleName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateTime JoinedUtc { get; set; }

    public bool IsActive { get; set; }

    public string? IdentityUserId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
