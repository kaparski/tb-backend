using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.StateIds.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.UnitTests.Controllers.StateIds.Requests;

public sealed class StateIdRequestValidatorTests
{
    private readonly StateIdRequestValidator _stateIdRequestValidator = new();

    [Fact]
    public async Task ValidationAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var stateIdRequest = TestData.StateIdRequestFaker.Generate();

        // Act
        var actualResult = await _stateIdRequestValidator.TestValidateAsync(stateIdRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ValidationAsync_EmptyStateIdCode_ShouldHaveErrors(string stateIdCode)
    {
        // Arrange
        var stateIdRequest = TestData.StateIdRequestFaker
            .RuleFor(s => s.StateIdCode, _ => stateIdCode)
            .Generate();

        // Act
        var actualResult = await _stateIdRequestValidator.TestValidateAsync(stateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.StateIdCode);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidationAsync_LongStateIdCode_ShouldHaveErrors()
    {
        // Arrange
        var stateIdRequest = TestData.StateIdRequestFaker
            .RuleFor(s => s.StateIdCode, f => f.Lorem.Random.AlphaNumeric(26))
            .Generate();

        // Act
        var actualResult = await _stateIdRequestValidator.TestValidateAsync(stateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.StateIdCode);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidationAsync_LongStateLocalJurisdiction_ShouldHaveErrors()
    {
        // Arrange
        var stateIdRequest = TestData.StateIdRequestFaker
            .RuleFor(s => s.LocalJurisdiction, f => f.Lorem.Random.AlphaNumeric(101))
            .Generate();

        // Act
        var actualResult = await _stateIdRequestValidator.TestValidateAsync(stateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.LocalJurisdiction);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidationAsync_StateEqualsNone_ShouldHaveErrors()
    {
        // Arrange
        var stateIdRequest = TestData.StateIdRequestFaker
            .RuleFor(s => s.State, _ => State.None)
            .Generate();

        // Act
        var actualResult = await _stateIdRequestValidator.TestValidateAsync(stateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.State);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidationAsync_StateIdTypeEqualsNone_ShouldHaveErrors()
    {
        // Arrange
        var stateIdRequest = TestData.StateIdRequestFaker
            .RuleFor(s => s.StateIdType, _ => StateIdType.None)
            .Generate();

        // Act
        var actualResult = await _stateIdRequestValidator.TestValidateAsync(stateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.StateIdType);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    private static class TestData
    {
        public static Faker<StateIdRequest> StateIdRequestFaker =>
            new Faker<StateIdRequest>()
                .RuleFor(s => s.State, f => f.PickRandomWithout(State.None))
                .RuleFor(s => s.StateIdType, f =>
                {
                    var list = StateIdType.List.Except(new[] { StateIdType.None });
                    return f.PickRandom(list);
                })
                .RuleFor(s => s.StateIdCode, f => f.Lorem.Random.AlphaNumeric(25))
                .RuleFor(s => s.LocalJurisdiction, f => f.Lorem.Random.AlphaNumeric(100));
    }
}
