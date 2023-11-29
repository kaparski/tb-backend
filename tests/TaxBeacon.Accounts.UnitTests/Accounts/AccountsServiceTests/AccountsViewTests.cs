using FluentAssertions.Execution;
using FluentAssertions;
using System.Text.RegularExpressions;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public void AccountsView_ListOfColumnsMatchesAccountViewEntity()
    {
        // Arrange
        var usersViewScript = File.ReadAllText("../../../../../migration-scripts/AccountsView.sql");

        var fieldsAsString = new Regex(@"select((.|\n)*?)from", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Match(usersViewScript)
            .Groups[1]
            .Value;

        var fields = new Regex(@"(\w+),?[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Matches(fieldsAsString)
            .Select(m => m.Groups[1].Value)
            .ToArray();

        var props = typeof(AccountView).GetProperties()
            .Select(p => p.Name)
            .ToArray();

        // Assert
        using (new AssertionScope())
        {
            fields.Should().BeEquivalentTo(props);
        }
    }
}
