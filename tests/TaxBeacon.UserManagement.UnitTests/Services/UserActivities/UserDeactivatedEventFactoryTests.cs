using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public class UserDeactivatedEventFactoryTests
    {
        private readonly IUserActivityFactory _sut;

        public UserDeactivatedEventFactoryTests() => _sut = new UserDeactivatedEventFactory();

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var deactivatedById = Guid.NewGuid();
            var date = DateTime.UtcNow;
            var userEvent = new UserDeactivatedEvent(deactivatedById, date, "Test", "Admin");

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(date);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be("User deactivated");
            };

        }
    }
}
