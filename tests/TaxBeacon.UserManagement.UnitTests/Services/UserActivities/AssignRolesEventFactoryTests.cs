using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System.Text.Json;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public sealed class AssignRolesEventFactoryTests
    {
        private readonly Mock<IDateTimeFormatter> _dateTimeFormatter;

        private readonly IUserActivityFactory _sut;

        public AssignRolesEventFactoryTests()
        {
            _dateTimeFormatter = new();

            _dateTimeFormatter.Setup(x => x.FormatDate(It.IsAny<DateTime>())).Returns(string.Empty);

            _sut = new AssignRolesEventFactory(_dateTimeFormatter.Object);
        }

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var assignedByUserId = Guid.NewGuid();
            var previousRoles = new List<RoleActivityDto> { new RoleActivityDto { Name = "Admin" } };
            var currentUserRoles = new List<RoleActivityDto> { new RoleActivityDto { Name = "Test" } };
            var userEvent = new AssignRolesEvent("Admin",
                assignedByUserId,
                "Test",
                previousRoles,
                currentUserRoles,
                DateTime.UtcNow);

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(string.Empty);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be(" User has been assigned to the following roles: Test by Test Admin");
            };

        }
    }
}
