using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.UserManagement.Services.Tenants.Activities;
using TaxBeacon.UserManagement.Services.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.Tenants.Activities;

public class TenantUnAssignProgramsEventFactoryTests
{
    private readonly ITenantActivityFactory _activityFactory;

    public TenantUnAssignProgramsEventFactoryTests() => _activityFactory = new TenantUnAssignProgramsEventFactory();

    [Fact]
    public void Create_heckMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var tenantAssignProgramsEvent = new TenantUnassignProgramsEvent(Guid.NewGuid(),
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
            result.Message.Should().Be("Program(s) access removed from Tenant: Test");
        };
    }
}
