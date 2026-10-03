using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Api.Models.Authors;
using Library.Data.Entities;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Library.Api.Security;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/authors")]
[Authorize]
public sealed class AuthorsController(LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<AuthorResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAuthors(
        CancellationToken cancellationToken)
    {
        var authors = await dbContext.Authors
            .AsNoTracking()
            .Select(author => new AuthorResponse
            {
                Id = author.Id,
                FirstName = author.FirstName,
                MiddleName = author.MiddleName,
                LastName = author.LastName,
                YearOfBirth = author.YearOfBirth,
                YearOfDeath = author.YearOfDeath,
                Country = author.Country,
                Biography = author.Biography,
                CreatedUtc = author.CreatedUtc,
                UpdatedUtc = author.UpdatedUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(authors);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<AuthorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorResponse>> GetAuthor(
    int id,
    CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(
                author => author.Id == id,
                cancellationToken);

        if (author is null)
        {
            return NotFound();
        }

        var response = MapToResponse(author);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<AuthorResponse>(StatusCodes.Status201Created)]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<AuthorResponse>> CreateAuthor(
    CreateAuthorRequest request,
    CancellationToken cancellationToken)
    {
        var author = new Author
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            YearOfBirth = request.YearOfBirth,
            YearOfDeath = request.YearOfDeath,
            Country = request.Country,
            Biography = request.Biography
        };

        dbContext.Authors.Add(author);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(author);

        return CreatedAtAction(
            nameof(GetAuthor),
            new { id = author.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<AuthorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<AuthorResponse>> UpdateAuthor(
    int id,
    UpdateAuthorRequest request,
    CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors
            .FirstOrDefaultAsync(
                author => author.Id == id,
                cancellationToken);

        if (author is null)
        {
            return NotFound();
        }

        author.FirstName = request.FirstName;
        author.MiddleName = request.MiddleName;
        author.LastName = request.LastName;
        author.YearOfBirth = request.YearOfBirth;
        author.YearOfDeath = request.YearOfDeath;
        author.Country = request.Country;
        author.Biography = request.Biography;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(author);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteAuthor(
    int id,
    CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors
            .FirstOrDefaultAsync(
                author => author.Id == id,
                cancellationToken);

        if (author is null)
        {
            return NotFound();
        }

        dbContext.Authors.Remove(author);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static AuthorResponse MapToResponse(Author author)
    {
        return new AuthorResponse
        {
            Id = author.Id,
            FirstName = author.FirstName,
            MiddleName = author.MiddleName,
            LastName = author.LastName,
            YearOfBirth = author.YearOfBirth,
            YearOfDeath = author.YearOfDeath,
            Country = author.Country,
            Biography = author.Biography,
            CreatedUtc = author.CreatedUtc,
            UpdatedUtc = author.UpdatedUtc
        };
    }
}