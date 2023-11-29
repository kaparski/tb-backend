using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Accounts.Entities.Exporters;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.UnitTests.Entities.Exporters;
public class AccountEntitiesToXlsxExporterTests
{
    private readonly AccountEntitiesToXlsxExporter _exporter;
    public AccountEntitiesToXlsxExporterTests() => _exporter = new AccountEntitiesToXlsxExporter();

    [Fact]
    public void Export_Successful_ReturnsFileStreamDto()
    {
        // Arrange
        var model = new AccountEntitiesExportModel
        {
            Entities = TestData.EntityFaker.Generate(4),
        };
        // Act
        var result = _exporter.Export(model);
        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            if (result != null)
            {
                using var stream = result.FileStream;
                stream.Should().NotBeNull().And.BeReadable().And.HavePosition(0);
                result.FileName.Should().Be("entities.xlsx");
                result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<EntityRow> EntityFaker =
            new Faker<EntityRow>()
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Type, AccountEntityType.LLC.Name)
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.State, State.NM.ToString())
                .RuleFor(t => t.Address1, f => f.Address.StreetAddress())
                .RuleFor(e => e.Fein, (f) => f.Random.Number(10000, 999999999).ToString())
                .RuleFor(t => t.County, f => f.Address.County())
                .RuleFor(t => t.DateOfIncorporationView, _ => DateTime.UtcNow.ToString())
                .RuleFor(t => t.NaicsView, f => $"{f.Random.Int()} - {f.Commerce.Categories(1).First()}")
                .RuleFor(t => t.Country, f => f.PickRandom<Country>(Country.List).ToString());
    }
}
