using Library.Api.Models.Borrowers;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Api.Security;
using Microsoft.AspNetCore.Authorization;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/borrowers")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class BorrowersController(
    LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<BorrowerResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BorrowerResponse>>> GetBorrowers(
        CancellationToken cancellationToken)
    {
        var borrowers = await dbContext.Borrowers
            .AsNoTracking()
            .Select(borrower => new BorrowerResponse
            {
                Id = borrower.Id,
                MembershipNumber = borrower.MembershipNumber,
                FirstName = borrower.FirstName,
                MiddleName = borrower.MiddleName,
                LastName = borrower.LastName,
                Email = borrower.Email,
                PhoneNumber = borrower.PhoneNumber,
                DateOfBirth = borrower.DateOfBirth,
                JoinedUtc = borrower.JoinedUtc,
                IsActive = borrower.IsActive,
                IdentityUserId = borrower.IdentityUserId,
                CreatedUtc = borrower.CreatedUtc,
                UpdatedUtc = borrower.UpdatedUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(borrowers);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<BorrowerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowerResponse>> GetBorrower(
    int id,
    CancellationToken cancellationToken)
    {
        var borrower = await dbContext.Borrowers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                borrower => borrower.Id == id,
                cancellationToken);

        if (borrower is null)
        {
            return NotFound();
        }

        var response = MapToResponse(borrower);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<BorrowerResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<BorrowerResponse>> CreateBorrower(
    CreateBorrowerRequest request,
    CancellationToken cancellationToken)
    {
        var borrower = new Borrower
        {
            MembershipNumber = request.MembershipNumber,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            JoinedUtc = request.JoinedUtc,
            IsActive = request.IsActive
        };

        dbContext.Borrowers.Add(borrower);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(borrower);

        return CreatedAtAction(
            nameof(GetBorrower),
            new { id = borrower.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<BorrowerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BorrowerResponse>> UpdateBorrower(
    int id,
    UpdateBorrowerRequest request,
    CancellationToken cancellationToken)
    {
        var borrower = await dbContext.Borrowers
            .FirstOrDefaultAsync(
                borrower => borrower.Id == id,
                cancellationToken);

        if (borrower is null)
        {
            return NotFound();
        }

        borrower.MembershipNumber = request.MembershipNumber;
        borrower.FirstName = request.FirstName;
        borrower.MiddleName = request.MiddleName;
        borrower.LastName = request.LastName;
        borrower.Email = request.Email;
        borrower.PhoneNumber = request.PhoneNumber;
        borrower.DateOfBirth = request.DateOfBirth;
        borrower.JoinedUtc = request.JoinedUtc;
        borrower.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(borrower);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBorrower(
    int id,
    CancellationToken cancellationToken)
    {
        var borrower = await dbContext.Borrowers
            .FirstOrDefaultAsync(
                borrower => borrower.Id == id,
                cancellationToken);

        if (borrower is null)
        {
            return NotFound();
        }

        dbContext.Borrowers.Remove(borrower);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static BorrowerResponse MapToResponse(Borrower borrower)
    {
        return new BorrowerResponse
        {
            Id = borrower.Id,
            MembershipNumber = borrower.MembershipNumber,
            FirstName = borrower.FirstName,
            MiddleName = borrower.MiddleName,
            LastName = borrower.LastName,
            Email = borrower.Email,
            PhoneNumber = borrower.PhoneNumber,
            DateOfBirth = borrower.DateOfBirth,
            JoinedUtc = borrower.JoinedUtc,
            IsActive = borrower.IsActive,
            IdentityUserId = borrower.IdentityUserId,
            CreatedUtc = borrower.CreatedUtc,
            UpdatedUtc = borrower.UpdatedUtc
        };
    }
}
