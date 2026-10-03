using Library.Api.Models.Books;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Library.Api.Security;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/books")]
[Authorize]
public sealed class BooksController(
    LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<BookResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooks(
        CancellationToken cancellationToken)
    {
        var books = await dbContext.Books
            .AsNoTracking()
            .Select(book => new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                YearOfFirstPublication = book.YearOfFirstPublication,
                Isbn = book.Isbn,
                Description = book.Description,
                LanguageCode = book.LanguageCode,
                PageCount = book.PageCount,
                AuthorId = book.AuthorId,
                CreatedUtc = book.CreatedUtc,
                UpdatedUtc = book.UpdatedUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> GetBook(
    int id,
    CancellationToken cancellationToken)
    {
        var book = await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(
                book => book.Id == id,
                cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        var response = MapToResponse(book);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<BookResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<BookResponse>> CreateBook(
    CreateBookRequest request,
    CancellationToken cancellationToken)
    {
        var authorExists = await dbContext.Authors
            .AnyAsync(
                author => author.Id == request.AuthorId,
                cancellationToken);

        if (!authorExists)
        {
            return BadRequest(
                $"Author with ID {request.AuthorId} does not exist.");
        }

        var book = new Book
        {
            Title = request.Title,
            YearOfFirstPublication = request.YearOfFirstPublication,
            Isbn = request.Isbn,
            Description = request.Description,
            LanguageCode = request.LanguageCode,
            PageCount = request.PageCount,
            AuthorId = request.AuthorId
        };

        dbContext.Books.Add(book);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(book);

        return CreatedAtAction(
            nameof(GetBook),
            new { id = book.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<BookResponse>> UpdateBook(
    int id,
    UpdateBookRequest request,
    CancellationToken cancellationToken)
    {
        var book = await dbContext.Books
            .FirstOrDefaultAsync(
                book => book.Id == id,
                cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        var authorExists = await dbContext.Authors
            .AnyAsync(
                author => author.Id == request.AuthorId,
                cancellationToken);

        if (!authorExists)
        {
            return BadRequest(
                $"Author with ID {request.AuthorId} does not exist.");
        }

        book.Title = request.Title;
        book.YearOfFirstPublication = request.YearOfFirstPublication;
        book.Isbn = request.Isbn;
        book.Description = request.Description;
        book.LanguageCode = request.LanguageCode;
        book.PageCount = request.PageCount;
        book.AuthorId = request.AuthorId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(book);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteBook(
    int id,
    CancellationToken cancellationToken)
    {
        var book = await dbContext.Books
            .FirstOrDefaultAsync(
                book => book.Id == id,
                cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        dbContext.Books.Remove(book);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static BookResponse MapToResponse(Book book)
    {
        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            YearOfFirstPublication = book.YearOfFirstPublication,
            Isbn = book.Isbn,
            Description = book.Description,
            LanguageCode = book.LanguageCode,
            PageCount = book.PageCount,
            AuthorId = book.AuthorId,
            CreatedUtc = book.CreatedUtc,
            UpdatedUtc = book.UpdatedUtc
        };
    }
}