using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf.Types;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Contacts;

public partial class ContactServiceTests
{
    [Fact]
    public async Task LinkContactToContactAsync_ContactsExistAndNotAssigned_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var firstContactActivityDate = DateTime.UtcNow.AddMinutes(-1);
        var secondContactActivityDate = DateTime.UtcNow.AddMinutes(1);
        _dateTimeServiceMock
            .SetupSequence(s => s.UtcNow)
            .Returns(firstContactActivityDate)
            .Returns(DateTime.UtcNow)
            .Returns(secondContactActivityDate)
            .Returns(DateTime.UtcNow);

        var expectedComment = new Faker().Lorem.Sentence();

        var expectedLinkedContacts = new List<LinkedContact>
        {
            new()
            {
                TenantId = tenant.Id,
                SourceContactId = contacts[0].Id,
                RelatedContactId = contacts[1].Id,
                Comment = expectedComment,
            },
            new()
            {
                TenantId = tenant.Id,
                RelatedContactId = contacts[0].Id,
                SourceContactId = contacts[1].Id,
                Comment = expectedComment,
            }
        };

        var expectedActivities = new List<ContactActivityLog>()
        {
            new()
            {
                TenantId = tenant.Id,
                ContactId = contacts[0].Id,
                Revision = 1,
                EventType = ContactEventType.ContactLinkedToContact,
                Date = firstContactActivityDate
            },
            new()
            {
                TenantId = tenant.Id,
                ContactId = contacts[1].Id,
                Revision = 1,
                EventType = ContactEventType.ContactLinkedToContact,
                Date = secondContactActivityDate
            }
        };

        // Act
        var actualResult = await _contactService
            .LinkContactToContactAsync(contacts[0].Id, contacts[1].Id, expectedComment);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var success, out _).Should().BeTrue();
            success.Should().BeAssignableTo<Success>();

            var actualLinkedContacts = await _dbContext.LinkedContacts.ToListAsync();
            actualLinkedContacts.Should().BeEquivalentTo(expectedLinkedContacts, opt =>
            {

                opt.Excluding(x => x.SourceContact);
                opt.Excluding(x => x.RelatedContact);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();

                return opt;
            });

            var actualActivities = await _dbContext.ContactActivityLogs.ToListAsync();
            actualActivities.Should().BeEquivalentTo(expectedActivities, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Contact);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();

                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(5));
        }
    }

    [Fact]
    public async Task LinkContactToContactAsync_ContactsNotExists_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var expectedComment = new Faker().Lorem.Sentence();

        // Act
        var actualResult = await _contactService
            .LinkContactToContactAsync(Guid.NewGuid(), Guid.NewGuid(), expectedComment);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task LinkContactToContactAsync_ContactsFromAnotherTenant_ReturnsNotFound()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenants[0].Id)
            .Generate(2);

        await _dbContext.Tenants.AddRangeAsync(tenants);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenants[1].Id);

        var expectedComment = new Faker().Lorem.Sentence();

        // Act
        var actualResult = await _contactService
            .LinkContactToContactAsync(contacts[0].Id, contacts[1].Id, expectedComment);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task LinkContactToContactAsync_ContactsAlreadyLinked_ReturnsConflict()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);

        var expectedComment = new Faker().Lorem.Sentence();

        var expectedLinkedContact = new LinkedContact
        {
            TenantId = tenant.Id,
            SourceContactId = contacts[0].Id,
            RelatedContactId = contacts[1].Id,
            Comment = expectedComment
        };

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.LinkedContacts.AddAsync(expectedLinkedContact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .LinkContactToContactAsync(contacts[0].Id, contacts[1].Id, expectedComment);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT2(out var conflict, out _).Should().BeTrue();
            conflict.Should().BeOfType<Conflict>();
        }
    }
}
