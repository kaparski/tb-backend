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
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.UnitTests.Entities.Exporters;
public class AccountEntitiesToCsvExporterTests
{
    private readonly Mock<IListToFileConverter> _csvMock = new();
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters = new();

    private readonly AccountEntitiesToCsvExporter _exporter;
    public AccountEntitiesToCsvExporterTests()
    {
        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object }.ToList()
                .GetEnumerator());

        _exporter = new AccountEntitiesToCsvExporter(_listToFileConverters.Object);
    }

    [Fact]
    public async Task Export_Successful_ReturnsFileStreamDto()
    {
        // Arrange
        var model = new AccountEntitiesExportModel
        {
            Entities = TestData.EntityFaker.Generate(4),
        };
        // Act
        var result = await _exporter.Export(model, CancellationToken.None);
        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            if (result != null)
            {
                using var stream = result.FileStream;
                stream.Should().NotBeNull().And.BeReadable().And.HavePosition(0);
                result.FileName.Should().Be("entities.zip");
                result.ContentType.Should().Be("application/zip");

                using var zip = new ZipArchive(stream);
                zip.Entries.Select(x => x.Name).Should().BeEquivalentTo(new[]{
                    "entities.csv",
                    "locations.csv",
                    "stateIds.csv"
                });
                _csvMock.Verify(x => x.Convert(It.IsAny<List<EntityRow>>()), Times.Once);
                _csvMock.Verify(x => x.Convert(It.IsAny<List<StateIdRow>>()), Times.Once);
                _csvMock.Verify(x => x.Convert(It.IsAny<List<LocationRow>>()), Times.Once);
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
