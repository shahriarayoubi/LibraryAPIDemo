using Library.Api.Models.Me;
using Library.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController(LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (identityUserId is null)
        {
            return Unauthorized();
        }

        var borrower = await dbContext.Borrowers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                borrower =>
                    borrower.IdentityUserId == identityUserId);

        if (borrower is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            borrower.Id,
            borrower.MembershipNumber,
            borrower.FirstName,
            borrower.MiddleName,
            borrower.LastName,
            borrower.Email
        });
    }

    [HttpGet("loans")]
    public async Task<IActionResult> GetLoans()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (identityUserId is null)
        {
            return Unauthorized();
        }

        var loans = await dbContext.Loans
            .AsNoTracking()
            .Where(loan =>
                loan.Borrower.IdentityUserId == identityUserId)
            .OrderByDescending(loan => loan.LoanedUtc)
            .Select(loan => new MyLoanResponse
            {
                LoanId = loan.Id,
                BookCopyId = loan.BookCopyId,
                BookTitle = loan.BookCopy.Book.Title,
                LoanedUtc = loan.LoanedUtc,
                DueUtc = loan.DueUtc,
                ReturnedUtc = loan.ReturnedUtc,
                RenewalCount = loan.RenewalCount
            })
            .ToListAsync();

        return Ok(loans);
    }

    [HttpGet("loans/current")]
    public async Task<IActionResult> GetCurrentLoans()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (identityUserId is null)
        {
            return Unauthorized();
        }

        var loans = await dbContext.Loans
            .AsNoTracking()
            .Where(loan =>
                loan.Borrower.IdentityUserId == identityUserId &&
                loan.ReturnedUtc == null)
            .OrderByDescending(loan => loan.LoanedUtc)
            .Select(loan => new MyLoanResponse
            {
                LoanId = loan.Id,
                BookCopyId = loan.BookCopyId,
                BookTitle = loan.BookCopy.Book.Title,
                LoanedUtc = loan.LoanedUtc,
                DueUtc = loan.DueUtc,
                ReturnedUtc = loan.ReturnedUtc,
                RenewalCount = loan.RenewalCount
            })
            .ToListAsync();

        return Ok(loans);
    }
}