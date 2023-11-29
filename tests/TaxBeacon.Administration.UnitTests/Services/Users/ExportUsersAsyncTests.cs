using Moq;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportUsersAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var tenant = UserServiceTests.TestData.TenantFaker.Generate();
        var users = UserServiceTests.TestData.UserFaker.Generate(5);

        foreach (var user in users)
        {
            user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        _ = await _userService.ExportUsersAsync(fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<TenantUserExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<TenantUserExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}
