using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using System.Text.RegularExpressions;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.UnitTests.Services;

public class GlobalSearchServiceTests
{
    [Theory]
    [InlineData("TenantsSearchView")]
    [InlineData("UsersSearchView")]
    [InlineData("RolesSearchView")]
    [InlineData("DivisionsSearchView")]
    [InlineData("DepartmentsSearchView")]
    [InlineData("ServiceAreasSearchView")]
    [InlineData("JobTitlesSearchView")]
    [InlineData("TeamsSearchView")]
    [InlineData("ClientsSearchView")]
    [InlineData("EntitiesSearchView")]
    [InlineData("LocationsSearchView")]
    [InlineData("ContactsSearchView")]
    public void IndexContainsFieldsFromViews(string viewName)
    {
        // Arrange
        var viewScript = File.ReadAllText($"../../../../../global-search/views/{viewName}.sql");

        var fieldsAsString = new Regex(@"select((.|\n)*)from", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Match(viewScript)
            .Groups[1]
            .Value;

        var fields = new Regex(@"(\w+),?[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Matches(fieldsAsString)
            .Select(m => m.Groups[1].Value)
            .Where(f => f != "IsDeleted")
            .ToArray();

        var props = typeof(SearchIndex).GetProperties()
            .Select(p => p.Name)
            .ToArray();

        // Assert
        using (new AssertionScope())
        {
            foreach (var field in fields)
            {
                props.Should().Contain(field);
            }
        }
    }

    [Fact]
    public void ProgramsSearchView_IndexContainsFieldsFromViews()
    {
        // Arrange

        var viewScript = File.ReadAllText($"../../../../../global-search/views/ProgramsSearchView.sql");

        var fieldsAsString = new Regex(@"union select((.|\n)*)from", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Match(viewScript)
            .Groups[1]
            .Value;

        var fields = new Regex(@"(\w+),?[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Matches(fieldsAsString)
            .Select(m => m.Groups[1].Value)
            .Where(f => f != "IsDeleted")
            .ToArray();

        var props = typeof(SearchIndex).GetProperties()
            .Select(p => p.Name)
            .ToArray();

        // Assert
        using (new AssertionScope())
        {
            foreach (var field in fields)
            {
                props.Should().Contain(field);
            }
        }
    }

    [Fact]
    public void SearchIndexContainsAllFieldsFromIndexDefinition()
    {
        // Arrange
        var json = File.ReadAllText($"../../../../../global-search/configs/indexSchema.json");
        var definitions = JsonSerializer.Deserialize<IndexDefinition[]>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var fields = definitions?[0].Fields.Select(f => f.Name).ToArray();

        var props = typeof(SearchIndex).GetProperties()
            .Select(p => p.Name)
            .ToArray();

        using (new AssertionScope())
        {
            props.Should().BeEquivalentTo(fields);
        }
    }

    [Fact]
    public void SearchIndexContainsAllSearchableFieldsFromIndexDefinition()
    {
        // Arrange
        var json = File.ReadAllText($"../../../../../global-search/configs/indexSchema.json");
        var definitions = JsonSerializer.Deserialize<IndexDefinition[]>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var fields = definitions?[0].Fields.Where(f => f.Searchable).Select(f => f.Name).ToArray();

        var props = SearchIndex.GetHighlightFields();

        using (new AssertionScope())
        {
            props.Should().BeEquivalentTo(fields);
        }
    }
}

internal class IndexDefinition
{
    public string Name { get; set; } = null!;

    public IndexField[] Fields { get; set; } = { };
}

internal class IndexField
{
    public string Name { get; set; } = null!;

    public bool Key { get; set; }

    public bool Searchable { get; set; }

    public bool Filterable { get; set; }

    public bool Sortable { get; set; }

    public bool Facetable { get; set; }

    public bool Retrievable { get; set; }
}
