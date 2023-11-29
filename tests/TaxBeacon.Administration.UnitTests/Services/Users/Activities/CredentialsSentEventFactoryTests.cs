using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Activities.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Users.Activities;

public sealed class CredentialsSentEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public CredentialsSentEventFactoryTests() => _sut = new CredentialsSentEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var createdById = Guid.NewGuid();
        var createdUserEmail = "test@test.com";
        var date = DateTime.UtcNow;
        var userEvent = new CredentialsSentEvent(createdById, createdUserEmail, date, "Test", "Admin");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Credentials sent");
        }
    }
}
