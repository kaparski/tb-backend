using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Accounts.Entities;
using Moq;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task CreateNewContact_AccountExistsAndValidData_ReturnsNewContact()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var contactCreatedDate = DateTime.UtcNow.AddMinutes(-1);
        var contactAssignedDate = DateTime.UtcNow.AddMinutes(1);
        _dateTimeServiceMock
            .SetupSequence(s => s.UtcNow)
            .Returns(contactCreatedDate)
            .Returns(DateTime.UtcNow)
            .Returns(contactAssignedDate)
            .Returns(DateTime.UtcNow)
            .Returns(DateTime.UtcNow);

        var createContactDto = TestData.CreateContactDtoFaker.Generate();

        var expectedActivities = new List<ContactActivityLog>()
        {
            new()
            {
                TenantId = tenant.Id,
                EventType = ContactEventType.ContactAssignedToAccount,
                Revision = 1,
                Date = contactAssignedDate
            },
            new()
            {
                TenantId = tenant.Id,
                EventType = ContactEventType.ContactCreated,
                Revision = 1,
                Date = contactCreatedDate
            },
        };

        // Act
        var actualResult = await _contactService.CreateNewContactAsync(account.Id, createContactDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var newContact, out _).Should().BeTrue();
            newContact.Should().BeEquivalentTo(createContactDto, opt =>
            {
                opt.Excluding(x => x.Type);
                opt.ExcludingMissingMembers();
                return opt;
            });
            newContact.Type.Should().BeEquivalentTo(createContactDto.Type.Name);
            newContact.Status.Should().Be(Status.Active);

            var actualActivityLog = await _dbContext.ContactActivityLogs.OrderByDescending(a => a.Date).ToListAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog.Should().BeEquivalentTo(expectedActivities, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Contact);
                opt.Excluding(x => x.Tenant);
                opt.Excluding(x => x.ContactId);
                opt.ExcludingMissingMembers();

                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(5));
        }
    }

    [Fact]
    public async Task CreateNewContact_AccountDoesNotExists_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var createContactDto = TestData.CreateContactDtoFaker.Generate();

        // Act
        var actualResult = await _contactService.CreateNewContactAsync(Guid.NewGuid(), createContactDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}
