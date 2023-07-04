using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.GlobalSearch.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.GlobalSearch.Requests;

public class SearchRequestValidatorTests
{
    private readonly SearchRequestValidator _searchRequestValidator;
    public SearchRequestValidatorTests() => _searchRequestValidator = new SearchRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var searchRequest = new Faker<SearchRequest>()
            .CustomInstantiator(f => new SearchRequest(f.Name.FirstName(), 1, 10))
            .Generate();

        //Act
        var actualResult = _searchRequestValidator.TestValidate(searchRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Text);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Page);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.PageSize);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1")]
    public void Validation_InvalidText_ShouldHaveTextErrors(string text)
    {
        //Arrange
        var searchRequest = new Faker<SearchRequest>()
            .CustomInstantiator(f => new SearchRequest(text, 1, 10))
            .Generate();

        //Act
        var actualResult = _searchRequestValidator.TestValidate(searchRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Text);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Page);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.PageSize);
    }

    [Fact]
    public void Validation_InvalidPage_ShouldHavePageErrors()
    {
        //Arrange
        var searchRequest = new Faker<SearchRequest>()
            .CustomInstantiator(f => new SearchRequest(f.Name.FirstName(), 0, 10))
            .Generate();

        //Act
        var actualResult = _searchRequestValidator.TestValidate(searchRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Text);
        actualResult.ShouldHaveValidationErrorFor(r => r.Page);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.PageSize);
    }

    [Fact]
    public void Validation_InvalidPageSize_ShouldHavePageSizeErrors()
    {
        //Arrange
        var searchRequest = new Faker<SearchRequest>()
            .CustomInstantiator(f => new SearchRequest(f.Name.FirstName(), 1, 0))
            .Generate();

        //Act
        var actualResult = _searchRequestValidator.TestValidate(searchRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Text);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Page);
        actualResult.ShouldHaveValidationErrorFor(r => r.PageSize);
    }
}
