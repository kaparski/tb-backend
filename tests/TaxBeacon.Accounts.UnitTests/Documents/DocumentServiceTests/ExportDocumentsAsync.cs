using Moq;
using TaxBeacon.Accounts.Documents.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Documents.Entities;
using FluentAssertions;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Accounts.UnitTests.Documents;
public partial class DocumentServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDocumentsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var user = TestData.UserFaker.Generate();
        var tenantUser = new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            User = user
        };
        var documents = TestData.DocumentFaker
            .RuleFor(d => d.TenantId, f => tenant.Id)
            .RuleFor(d => d.TenantUser, f => tenantUser)
            .RuleFor(d => d.UserId, f => tenantUser.UserId)
            .Generate(5)
            .Select(x => new AccountDocument
            {
                Tenant = tenant,
                TenantId = tenant.Id,
                DocumentId = x.Id,
                Document = x,
                Account = account,
                AccountId = account.Id
            });

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountDocuments.AddRangeAsync(documents);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        _ = await _documentService.ExportDocumentsAsync(account.Id, fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<DocumentExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DocumentExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task ExportDocumentsAsync_AccountDoesNotExist_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var user = TestData.UserFaker.Generate();
        var tenantUser = new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            User = user
        };
        var documents = TestData.DocumentFaker
            .RuleFor(d => d.TenantId, f => tenant.Id)
            .RuleFor(d => d.TenantUser, f => tenantUser)
            .RuleFor(d => d.UserId, f => tenantUser.UserId)
            .Generate(5)
            .Select(x => new AccountDocument
            {
                Tenant = tenant,
                TenantId = tenant.Id,
                DocumentId = x.Id,
                Document = x,
                Account = account,
                AccountId = account.Id
            });

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountDocuments.AddRangeAsync(documents);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _documentService.ExportDocumentsAsync(Guid.NewGuid(), FileType.Csv);

        //Assert
        result.IsT0.Should().BeFalse();
        result.IsT1.Should().BeTrue();
    }
}
