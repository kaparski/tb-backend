using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public class UserCreatedEventFactoryTests
    {
        private readonly IUserActivityFactory _sut;

        public UserCreatedEventFactoryTests() => _sut = new UserCreatedEventFactory();

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var createdById = Guid.NewGuid();
            var createdUserEmail = "test@test.com";
            var date = DateTime.UtcNow;
            var userEvent = new UserCreatedEvent(createdById, createdUserEmail, date, "Test", "Admin");

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(date);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be("User created");
            };

        }
    }
}
