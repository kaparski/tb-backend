using TaxBeacon.Common.Enums;
using TaxBeacon.Administration.Departments.Models;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;

public partial class DepartmentServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var departments = TestData.TestDepartment.Generate(5);

        await _dbContextMock.Departments.AddRangeAsync(departments);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _departmentService.ExportDepartmentsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<DepartmentExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DepartmentExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidInputDataWithDivision_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var departments = TestData.TestDepartment.Generate(5);

        await _dbContextMock.Departments.AddRangeAsync(departments);
        await _dbContextMock.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        _ = await _departmentService.ExportDepartmentsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<DepartmentWithDivisionExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DepartmentWithDivisionExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}
