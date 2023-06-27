using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities;
using TaxBeacon.Administration.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.Tenants.Activities;

public sealed class TenantAssignProgramsEventFactoryTests
{
    private readonly ITenantActivityFactory _activityFactory;

    public TenantAssignProgramsEventFactoryTests() => _activityFactory = new TenantAssignProgramsEventFactory();

    [Fact]
    public void Create_ValidateMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var tenantAssignProgramsEvent = new TenantAssignProgramsEvent(Guid.NewGuid(),
            "Super Admin",
            "Test",
            "Test",
            date);

        //Act
        var result = _activityFactory.Create(JsonSerializer.Serialize(tenantAssignProgramsEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Access to the following program(s) provided: Test");
        };
    }
}
