using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.Authentication;

public sealed class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public required string UserName { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    [MaxLength(320)]
    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}