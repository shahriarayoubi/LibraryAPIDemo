using Library.Api.Models.BookCopies;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Library.Api.Security;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/book-copies")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class BookCopiesController(
    LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<BookCopyResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookCopyResponse>>> GetBookCopies(
        CancellationToken cancellationToken)
    {
        var bookCopies = await dbContext.BookCopies
            .AsNoTracking()
            .Select(copy => new BookCopyResponse
            {
                Id = copy.Id,
                BookId = copy.BookId,
                InventoryNumber = copy.InventoryNumber,
                Status = copy.Status,
                ShelfLocation = copy.ShelfLocation,
                AcquiredUtc = copy.AcquiredUtc,
                CreatedUtc = copy.CreatedUtc,
                UpdatedUtc = copy.UpdatedUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(bookCopies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<BookCopyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookCopyResponse>> GetBookCopy(
    int id,
    CancellationToken cancellationToken)
    {
        var bookCopy = await dbContext.BookCopies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                copy => copy.Id == id,
                cancellationToken);

        if (bookCopy is null)
        {
            return NotFound();
        }

        var response = MapToResponse(bookCopy);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<BookCopyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookCopyResponse>> CreateBookCopy(
    CreateBookCopyRequest request,
    CancellationToken cancellationToken)
    {
        var bookExists = await dbContext.Books
            .AnyAsync(
                book => book.Id == request.BookId,
                cancellationToken);

        if (!bookExists)
        {
            return BadRequest(
                $"Book with ID {request.BookId} does not exist.");
        }

        var bookCopy = new BookCopy
        {
            BookId = request.BookId,
            InventoryNumber = request.InventoryNumber,
            Status = request.Status,
            ShelfLocation = request.ShelfLocation,
            AcquiredUtc = request.AcquiredUtc
        };

        dbContext.BookCopies.Add(bookCopy);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(bookCopy);

        return CreatedAtAction(
            nameof(GetBookCopy),
            new { id = bookCopy.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<BookCopyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookCopyResponse>> UpdateBookCopy(
    int id,
    UpdateBookCopyRequest request,
    CancellationToken cancellationToken)
    {
        var bookCopy = await dbContext.BookCopies
            .FirstOrDefaultAsync(
                copy => copy.Id == id,
                cancellationToken);

        if (bookCopy is null)
        {
            return NotFound();
        }

        var bookExists = await dbContext.Books
            .AnyAsync(
                book => book.Id == request.BookId,
                cancellationToken);

        if (!bookExists)
        {
            return BadRequest(
                $"Book with ID {request.BookId} does not exist.");
        }

        bookCopy.BookId = request.BookId;
        bookCopy.InventoryNumber = request.InventoryNumber;
        bookCopy.Status = request.Status;
        bookCopy.ShelfLocation = request.ShelfLocation;
        bookCopy.AcquiredUtc = request.AcquiredUtc;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(bookCopy);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBookCopy(
    int id,
    CancellationToken cancellationToken)
    {
        var bookCopy = await dbContext.BookCopies
            .FirstOrDefaultAsync(
                copy => copy.Id == id,
                cancellationToken);

        if (bookCopy is null)
        {
            return NotFound();
        }

        dbContext.BookCopies.Remove(bookCopy);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static BookCopyResponse MapToResponse(BookCopy bookCopy)
    {
        return new BookCopyResponse
        {
            Id = bookCopy.Id,
            BookId = bookCopy.BookId,
            InventoryNumber = bookCopy.InventoryNumber,
            Status = bookCopy.Status,
            ShelfLocation = bookCopy.ShelfLocation,
            AcquiredUtc = bookCopy.AcquiredUtc,
            CreatedUtc = bookCopy.CreatedUtc,
            UpdatedUtc = bookCopy.UpdatedUtc
        };
    }
}
