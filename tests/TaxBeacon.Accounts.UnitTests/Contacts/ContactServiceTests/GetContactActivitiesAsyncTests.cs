using FluentAssertions.Execution;
using FluentAssertions;
using OneOf.Types;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task GetContactActivitiesAsync_ContactExistsInDb_ReturnsActivities()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();
        var contactActivityLogs = TestData.ContactActivityFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.ContactId, _ => contact.Id)
            .Generate(2);
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.ContactActivityLogs.AddRangeAsync(contactActivityLogs);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.GetContactActivitiesAsync(contact.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activity, out _).Should().BeTrue();
            activity.Query.Count().Should().Be(contactActivityLogs.Count);
        }
    }

    [Fact]
    public async Task GetContactActivitiesAsync_ContactDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.GetContactActivitiesAsync(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}
