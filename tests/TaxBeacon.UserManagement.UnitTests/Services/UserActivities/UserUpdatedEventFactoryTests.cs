using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System.Text.Json;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities
{
    public class UserUpdatedEventFactoryTests
    {
        private readonly Mock<IDateTimeFormatter> _dateTimeFormatter;

        private readonly IUserActivityFactory _sut;

        public UserUpdatedEventFactoryTests()
        {
            _dateTimeFormatter = new();

            _dateTimeFormatter.Setup(x => x.FormatDate(It.IsAny<DateTime>())).Returns(string.Empty);

            _sut = new UserUpdatedEventFactory(_dateTimeFormatter.Object);
        }

        [Fact]
        public void Create_CheckMapping()
        {
            //Arrange
            var updatedById = Guid.NewGuid();
            var userEvent = new UserUpdatedEvent(updatedById, DateTime.UtcNow, "Test", "Admin", "previous", "current");

            //Act
            var result = _sut.Create(JsonSerializer.Serialize(userEvent));

            //Arrange
            using (new AssertionScope())
            {
                result.Date.Should().Be(string.Empty);
                result.FullName.Should().Be("Test");
                result.Message.Should().Be(" User details updated: previous to current by Test Admin");
            };

        }
    }
}
