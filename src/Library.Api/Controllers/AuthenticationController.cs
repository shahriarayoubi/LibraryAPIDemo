using Library.Api.Models.Authentication;
using Library.Api.Security;
using Library.Api.Services;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/authentication")]
public sealed class AuthenticationController(
    UserManager<IdentityUser> userManager,
    JwtTokenService jwtTokenService,
    LibraryDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);

        if (user is null)
        {
            return Unauthorized();
        }

        var passwordIsValid =
            await userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordIsValid)
        {
            return Unauthorized();
        }

        var token = await jwtTokenService.CreateTokenAsync(user);

        return Ok(new LoginResponse
        {
            UserName = user.UserName!,
            Token = token
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(
    RegisterRequest request)
    {

        var strategy =
            dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<ActionResult>(async () =>
        {
            await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

            var user = new IdentityUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var createResult = await userManager.CreateAsync(
                user,
                request.Password);

            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(createResult.Errors.Select(error =>
                    new
                    {
                        error.Code,
                        error.Description
                    }));
            }

            var roleResult = await userManager.AddToRoleAsync(
                user,
                RoleNames.Borrower);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(roleResult.Errors.Select(error =>
                    new
                    {
                        error.Code,
                        error.Description
                    }));
            }

            var borrower = new Borrower
            {
                MembershipNumber = MembershipNumberGenerator.Generate(),
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                JoinedUtc = DateTime.UtcNow,
                IsActive = true,
                IdentityUserId = user.Id
            };

            dbContext.Borrowers.Add(borrower);

            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok();
        });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new
        {
            Message = "You are authenticated as an Admin."
        });
    }
}