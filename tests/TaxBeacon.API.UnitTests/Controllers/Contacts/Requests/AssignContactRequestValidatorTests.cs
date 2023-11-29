using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts.Requests;

public class AssignContactRequestValidatorTests
{
    private readonly AssignContactRequestValidator _assignContactRequestValidator = new AssignContactRequestValidator();

    [Fact]
    public async Task ValidateAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var assignContactRequest = TestData.AssignContactRequestFaker.Generate();

        // Act
        var actualResult = await _assignContactRequestValidator.TestValidateAsync(assignContactRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_InvalidContactType_ShouldHaveNoErrors()
    {
        // Arrange
        var assignContactRequest = TestData.AssignContactRequestFaker
            .RuleFor(r => r.Type, _ => ContactType.None)
            .Generate();

        // Act
        var actualResult = await _assignContactRequestValidator.TestValidateAsync(assignContactRequest);

        // Assert
        actualResult.ShouldHaveValidationErrorFor(x => x.Type);
    }

    private static class TestData
    {
        public static Faker<AssignContactRequest> AssignContactRequestFaker => new Faker<AssignContactRequest>()
            .RuleFor(c => c.Type, f =>
            {
                var list = ContactType.List.Except(new[] { ContactType.None });
                return f.PickRandom(list);
            });
    }
}
