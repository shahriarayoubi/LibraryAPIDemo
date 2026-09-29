using Library.Data.Common;

namespace Library.Data.Entities;

public sealed class Borrower : IAuditableEntity
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

    public bool IsActive { get; set; } = true;

    /*
     * This can later point to the authentication user's identifier.
     * Keeping it nullable means authentication is not required in phase one.
     */
    public string? IdentityUserId { get; set; }

    public ICollection<Loan> Loans { get; set; } = [];
    public DateTime CreatedUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime UpdatedUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}