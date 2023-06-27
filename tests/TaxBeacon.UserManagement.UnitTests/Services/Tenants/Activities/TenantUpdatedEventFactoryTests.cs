using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities;
using TaxBeacon.Administration.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.Tenants.Activities;

public sealed class TenantUpdatedEventFactoryTests
{
    private readonly ITenantActivityFactory _activityFactory;

    public TenantUpdatedEventFactoryTests() => _activityFactory = new TenantUpdatedEventFactory();

    [Fact]
    public void Create_ValidateMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var tenantAssignProgramsEvent = new TenantUpdatedEvent(Guid.NewGuid(),
            "Super Admin",
            "Test",
            date,
            string.Empty,
            string.Empty);

        //Act
        var result = _activityFactory.Create(JsonSerializer.Serialize(tenantAssignProgramsEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Tenant details updated");
        };
    }
}
