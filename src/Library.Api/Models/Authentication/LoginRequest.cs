using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.Authentication;

public sealed class LoginRequest
{
    [Required]
    public required string UserName { get; set; }

    [Required]
    public required string Password { get; set; }
}