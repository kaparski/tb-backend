﻿using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.API.Controllers.Tenants.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant.Requests;

public sealed class ChangeTenantProgramsRequestValidatorTests
{
    private readonly ChangeTenantProgramsRequestValidator _changeTenantProgramsRequestValidator = new();

    [Theory]
    [MemberData(nameof(TestData.ValidProgramsIds), MemberType = typeof(TestData))]
    public void ChangeTenantProgramsRequest_ValidProgramsIds_ShouldReturnNoError(Guid[] programsIds)
    {
        // Arrange
        var request = new ChangeTenantProgramsRequest(programsIds);

        // Act
        var actualResult = _changeTenantProgramsRequestValidator.TestValidate(request);

        // Assert
        actualResult.ShouldNotHaveValidationErrorFor(x => x.ProgramsIds);
    }

    [Theory]
    [MemberData(nameof(TestData.InvalidProgramsIds), MemberType = typeof(TestData))]
    public void ChangeTenantProgramsRequest_ValidProgramsIds_ShouldReturnsError(Guid[] programsIds)
    {
        // Arrange
        var request = new ChangeTenantProgramsRequest(programsIds);

        // Act
        var actualResult = _changeTenantProgramsRequestValidator.TestValidate(request);

        // Assert
        actualResult.ShouldHaveValidationErrorFor(x => x.ProgramsIds);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static IEnumerable<object[]> ValidProgramsIds = new List<object[]>
        {
            new object[] { new[] { Guid.NewGuid() } },
            new object[] { new[] { Guid.NewGuid(), Guid.NewGuid() } },
        };

        public static IEnumerable<object?[]> InvalidProgramsIds = new List<object?[]>
        {
            new object?[] { null },
            new object[] { Array.Empty<Guid>() },
        };
    }
}
