using FluentAssertions.Execution;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.UnitTests.Accounts;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL;
using TaxBeacon.Accounts.Common.Services;
using TaxBeacon.Accounts.Entities.Models;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Common;
public class CsvServiceTests
{
    private readonly ICsvService _csvService = new CsvService();

    [Fact]
    public void Read_ValidData_ReturnList()
    {
        // Arrange
        Stream stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, 500, true))
        {
            writer.WriteLine("Entity Name*,Entity ID,DBA,Country*,Address 1**,Address 2,City**,County**,State**,Zip**,Address***,Phone,Entity Type*,Tax Year End Type,Date Of Incorporation,FEIN,EIN,NAICS Code");
            writer.WriteLine("Dicki and Sons,,window cleaner,International,,,,,,,4327 Emiko Hill,,C-Corp,,,,,");
            writer.WriteLine("Koch-Farrell,,teacher,International,,,,,,,794 Labadie Turnpike,,C-Corp,,,,,");
        }
        stream.Position = 0;

        // Act
        var actualResult = _csvService.Read<ImportEntityModel>(stream).ToList();

        // Assert
        actualResult.Count.Should().Be(2);
    }
}
