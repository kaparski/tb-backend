using Bogus;
using FluentAssertions;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Contacts.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts.Requests;

public sealed class LinkContactRequestTests
{
    private readonly LinkContactRequestValidator _linkContactRequestValidator = new();

    [Fact]
    public async Task ValidateAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var linkContactRequest = TestData.LinkContactRequestFaker.Generate();

        // Act
        var actualResult = await _linkContactRequestValidator.TestValidateAsync(linkContactRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ValidateAsync_EmptyComment_ShouldHaveErrors(string comment)
    {
        // Arrange
        var linkContactRequest = TestData.LinkContactRequestFaker
            .RuleFor(r => r.Comment, _ => comment)
            .Generate();

        // Act
        var actualResult = await _linkContactRequestValidator.TestValidateAsync(linkContactRequest);

        // Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Comment);
        actualResult.Errors.Count.Should().Be(1);
    }

    [Fact]
    public async Task ValidateAsync_LongComment_ShouldHaveErrors()
    {
        // Arrange
        var linkContactRequest = TestData.LinkContactRequestFaker
            .RuleFor(r => r.Comment, f => f.Lorem.Random.AlphaNumeric(200))
            .Generate();

        // Act
        var actualResult = await _linkContactRequestValidator.TestValidateAsync(linkContactRequest);

        // Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Comment)
            .WithErrorMessage("The comment must contain no more than 150");
        actualResult.Errors.Count.Should().Be(1);
    }

    private static class TestData
    {
        public static Faker<LinkContactRequest> LinkContactRequestFaker => new Faker<LinkContactRequest>()
            .RuleFor(r => r.RelatedContactId, _ => new Guid())
            .RuleFor(r => r.Comment, f => f.Lorem.Sentence(2));
    }
}
