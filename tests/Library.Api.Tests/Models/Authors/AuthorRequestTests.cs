using System.ComponentModel.DataAnnotations;
using Library.Api.Models.Authors;

namespace Library.Api.Tests.Models.Authors;

public sealed class AuthorRequestTests
{
    [Fact]
    public void Validate_WhenYearOfDeathIsBeforeYearOfBirth_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateAuthorRequest
        {
            FirstName = "George",
            LastName = "Orwell",
            YearOfBirth = 1950,
            YearOfDeath = 1940
        };

        var validationContext = new ValidationContext(request);

        // Act
        var results = request
            .Validate(validationContext)
            .ToList();

        // Assert
        var result = Assert.Single(results);

        Assert.Equal(
            "Year of death cannot be earlier than year of birth.",
            result.ErrorMessage);
    }

    [Fact]
    public void Validate_WhenYearOfDeathIsAfterYearOfBirth_ReturnsNoValidationErrors()
    {
        // Arrange
        var request = new CreateAuthorRequest
        {
            FirstName = "George",
            LastName = "Orwell",
            YearOfBirth = 1903,
            YearOfDeath = 1950
        };

        var validationContext = new ValidationContext(request);

        // Act`
        var results = request
            .Validate(validationContext)
            .ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WhenYearOfDeathEqualsYearOfBirth_ReturnsNoValidationErrors()
    {
        // Arrange
        var request = new CreateAuthorRequest
        {
            FirstName = "Example",
            LastName = "Author",
            YearOfBirth = 1900,
            YearOfDeath = 1900
        };

        var validationContext = new ValidationContext(request);

        // Act
        var results = request
            .Validate(validationContext)
            .ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WhenYearOfDeathIsNull_ReturnsNoValidationErrors()
    {
        // Arrange
        var request = new CreateAuthorRequest
        {
            FirstName = "Example",
            LastName = "Author",
            YearOfBirth = 1900,
            YearOfDeath = null
        };

        var validationContext = new ValidationContext(request);

        // Act
        var results = request
            .Validate(validationContext)
            .ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Validation_WhenFirstNameExceeds100Characters_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateAuthorRequest
        {
            FirstName = new string('A', 101),
            LastName = "Author"
        };

        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            request,
            validationContext,
            results,
            validateAllProperties: true);

        // Assert
        Assert.False(isValid);

        var result = Assert.Single(results);

        Assert.Contains(
            nameof(request.FirstName),
            result.MemberNames);
    }

}