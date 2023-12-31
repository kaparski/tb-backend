﻿using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities;
using TaxBeacon.Administration.Tenants.Activities.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants.Activities;

public sealed class TenantEnteredEventFactoryTests
{
    private readonly ITenantActivityFactory _activityFactory;

    public TenantEnteredEventFactoryTests() => _activityFactory = new TenantEnteredEventFactory();

    [Fact]
    public void Create_ValidateMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var tenantAssignProgramsEvent = new TenantEnteredEvent(Guid.NewGuid(),
            "Super Admin",
            "Test",
            date);

        //Act
        var result = _activityFactory.Create(JsonSerializer.Serialize(tenantAssignProgramsEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("User entered the tenant");
        };
    }
}
