using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public sealed class AssignRolesEventFactoryTests
    {
        private readonly IUserActivityFactory _sut;

        public AssignRolesEventFactoryTests() => _sut = new AssignRolesEventFactory();

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var assignedByUserId = Guid.NewGuid();
            var date = DateTime.UtcNow;
            var previousRoles = new List<RoleActivityDto> { new RoleActivityDto { Name = "Admin" } };
            var currentUserRoles = new List<RoleActivityDto> { new RoleActivityDto { Name = "Test" } };
            var userEvent = new AssignRolesEvent("Admin",
                assignedByUserId,
                "Test",
                previousRoles,
                currentUserRoles,
                date);

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(date);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be("User has been assigned to the following role(s): Test");
            };

        }
    }
}
