using Library.Api.Models.Loans;
using Library.Api.Security;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/loans")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class LoansController(
    LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<LoanResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanResponse>>> GetLoans(
        CancellationToken cancellationToken)
    {
        var loans = await dbContext.Loans
            .AsNoTracking()
            .Select(loan => new LoanResponse
            {
                Id = loan.Id,
                BookCopyId = loan.BookCopyId,
                BorrowerId = loan.BorrowerId,
                LoanedUtc = loan.LoanedUtc,
                DueUtc = loan.DueUtc,
                ReturnedUtc = loan.ReturnedUtc,
                RenewedUtc = loan.RenewedUtc,
                RenewalCount = loan.RenewalCount,
                Notes = loan.Notes,
                CreatedUtc = loan.CreatedUtc,
                UpdatedUtc = loan.UpdatedUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(loans);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(
    int id,
    CancellationToken cancellationToken)
    {
        var loan = await dbContext.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(
                loan => loan.Id == id,
                cancellationToken);

        if (loan is null)
        {
            return NotFound();
        }

        var response = MapToResponse(loan);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> CreateLoan(
    CreateLoanRequest request,
    CancellationToken cancellationToken)
    {
        var bookCopy = await dbContext.BookCopies
            .SingleOrDefaultAsync(
                copy => copy.Id == request.BookCopyId,
                cancellationToken);

        if (bookCopy is null)
        {
            return BadRequest(
                $"Book copy with ID {request.BookCopyId} does not exist.");
        }

        if (bookCopy.Status != BookCopyStatus.Available)
        {
            return BadRequest(
                $"Book copy with ID {request.BookCopyId} is not available.");
        }

        var borrowerExists = await dbContext.Borrowers
            .AnyAsync(
                borrower => borrower.Id == request.BorrowerId,
                cancellationToken);

        if (!borrowerExists)
        {
            return BadRequest(
                $"Borrower with ID {request.BorrowerId} does not exist.");
        }

        var loan = new Loan
        {
            BookCopyId = request.BookCopyId,
            BorrowerId = request.BorrowerId,
            LoanedUtc = request.LoanedUtc,
            DueUtc = request.DueUtc,
            Notes = request.Notes
        };

        dbContext.Loans.Add(loan);

        bookCopy.Status = BookCopyStatus.OnLoan;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(loan);

        return CreatedAtAction(
            nameof(GetLoan),
            new { id = loan.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<LoanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> UpdateLoan(
    int id,
    UpdateLoanRequest request,
    CancellationToken cancellationToken)
    {
        var loan = await dbContext.Loans
            .FirstOrDefaultAsync(
                loan => loan.Id == id,
                cancellationToken);

        if (loan is null)
        {
            return NotFound();
        }

        var bookCopyExists = await dbContext.BookCopies
            .AnyAsync(
                copy => copy.Id == request.BookCopyId,
                cancellationToken);

        if (!bookCopyExists)
        {
            return BadRequest(
                $"Book copy with ID {request.BookCopyId} does not exist.");
        }

        var borrowerExists = await dbContext.Borrowers
            .AnyAsync(
                borrower => borrower.Id == request.BorrowerId,
                cancellationToken);

        if (!borrowerExists)
        {
            return BadRequest(
                $"Borrower with ID {request.BorrowerId} does not exist.");
        }

        loan.BookCopyId = request.BookCopyId;
        loan.BorrowerId = request.BorrowerId;
        loan.LoanedUtc = request.LoanedUtc;
        loan.DueUtc = request.DueUtc;
        loan.ReturnedUtc = request.ReturnedUtc;
        loan.RenewedUtc = request.RenewedUtc;
        loan.RenewalCount = request.RenewalCount;
        loan.Notes = request.Notes;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(loan);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLoan(
    int id,
    CancellationToken cancellationToken)
    {
        var loan = await dbContext.Loans
            .FirstOrDefaultAsync(
                loan => loan.Id == id,
                cancellationToken);

        if (loan is null)
        {
            return NotFound();
        }

        dbContext.Loans.Remove(loan);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static LoanResponse MapToResponse(Loan loan)
    {
        return new LoanResponse
        {
            Id = loan.Id,
            BookCopyId = loan.BookCopyId,
            BorrowerId = loan.BorrowerId,
            LoanedUtc = loan.LoanedUtc,
            DueUtc = loan.DueUtc,
            ReturnedUtc = loan.ReturnedUtc,
            RenewedUtc = loan.RenewedUtc,
            RenewalCount = loan.RenewalCount,
            Notes = loan.Notes,
            CreatedUtc = loan.CreatedUtc,
            UpdatedUtc = loan.UpdatedUtc
        };
    }
}
