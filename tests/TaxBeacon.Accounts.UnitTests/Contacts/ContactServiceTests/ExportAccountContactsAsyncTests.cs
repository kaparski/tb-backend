using Moq;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountContactsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var items = TestData.ContactFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(5);

        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        _ = await _contactService.ExportAccountContactsAsync(account.Id, fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<AccountContactExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<AccountContactExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task ExportAccountContactsAsync_AccountDoesNotExist_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var items = TestData.ContactFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(5);

        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _contactService.ExportAccountContactsAsync(Guid.NewGuid(), FileType.Csv);

        //Assert
        result.IsT0.Should().BeFalse();
        result.IsT1.Should().BeTrue();
    }
}
