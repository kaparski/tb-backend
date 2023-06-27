using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities;
using TaxBeacon.Administration.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.Tenants.Activities;

public sealed class TenantExitedEventFactoryTests
{
    private readonly ITenantActivityFactory _activityFactory;

    public TenantExitedEventFactoryTests() => _activityFactory = new TenantExitedEventFactory();

    [Fact]
    public void Create_ValidateMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var tenantAssignProgramsEvent = new TenantExitedEvent(Guid.NewGuid(),
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
            result.Message.Should().Be("User exited the tenant");
        };
    }
}
