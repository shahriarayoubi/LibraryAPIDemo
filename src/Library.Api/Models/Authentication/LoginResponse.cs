namespace Library.Api.Models.Authentication;

public sealed class LoginResponse
{
    public required string UserName { get; set; }

    public required string Token { get; set; }
}