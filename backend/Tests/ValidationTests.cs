using FluentValidation.Results;
using MatdanSathi.API.Application.Voters.Queries.CheckVoterRegistration;
using Xunit;

namespace MatdanSathi.API.Tests;

public class ValidationTests
{
    private readonly CheckVoterRegistrationQueryValidator _validator;

    public ValidationTests()
    {
        _validator = new CheckVoterRegistrationQueryValidator();
    }

    [Fact]
    public void Validator_Should_Pass_For_ValidQuery()
    {
        // Arrange
        var query = new CheckVoterRegistrationQuery("ABC1234567", "User123", "PWA_App");

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("VERYLONGSTRINGTHATEXCEEDSTWENTYCHARACTERS")]
    [InlineData("ABC123#")] // contains invalid character '#'
    public void Validator_Should_Fail_For_InvalidEpicNumber(string? epicNumber)
    {
        // Arrange
        var query = new CheckVoterRegistrationQuery(epicNumber!, "User123", "PWA_App");

        // Act
        ValidationResult result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CheckVoterRegistrationQuery.EpicNumber));
    }
}
