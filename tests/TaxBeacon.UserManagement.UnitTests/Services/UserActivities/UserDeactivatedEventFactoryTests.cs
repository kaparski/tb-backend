using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System.Text.Json;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public class UserDeactivatedEventFactoryTests
    {
        private readonly Mock<IDateTimeFormatter> _dateTimeFormatter;

        private readonly IUserActivityFactory _sut;

        public UserDeactivatedEventFactoryTests()
        {
            _dateTimeFormatter = new();

            _dateTimeFormatter.Setup(x => x.FormatDate(It.IsAny<DateTime>())).Returns(string.Empty);

            _sut = new UserDeactivatedEventFactory(_dateTimeFormatter.Object);
        }

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var deactivatedById = Guid.NewGuid();
            var userEvent = new UserDeactivatedEvent(deactivatedById, DateTime.UtcNow, "Test", "Admin");

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(string.Empty);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be(" User deactivated by Test Admin");
            };

        }
    }
}
