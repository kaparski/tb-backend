using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System.Text.Json;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public class UserCreatedEventFactoryTests
    {
        private readonly Mock<IDateTimeFormatter> _dateTimeFormatter;

        private readonly IUserActivityFactory _sut;

        public UserCreatedEventFactoryTests()
        {
            _dateTimeFormatter = new();

            _dateTimeFormatter.Setup(x => x.FormatDate(It.IsAny<DateTime>())).Returns(string.Empty);

            _sut = new UserCreatedEventFactory(_dateTimeFormatter.Object);
        }

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var createdById = Guid.NewGuid();
            var createdUserEmail = "test@test.com";
            var userEvent = new UserCreatedEvent(createdById, createdUserEmail, DateTime.UtcNow, "Test", "Admin");

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(string.Empty);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be(" User created by Test Admin");
            };

        }
    }
}
