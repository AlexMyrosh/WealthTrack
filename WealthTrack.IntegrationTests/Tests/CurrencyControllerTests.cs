using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("CurrencyTests")]
public class CurrencyControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET ALL TESTS

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_ShouldReturnCorrectNumberOfCurrencies(int numberOfCurrencies)
    {
        // Arrange
        var currencies = DataFactory.CreateManyCurrencies(numberOfCurrencies);
        DbContext.Currencies.AddRange(currencies);
        await DbContext.SaveChangesAsync();
        var currencyIds = currencies.Select(c => c.Id).ToList();
        
        // Act
        var response = await Client.GetAsync("/api/currency");
        var currenciesFromResponse = await response.Content.ReadFromJsonAsync<List<CurrencyDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        currenciesFromResponse.Should().NotBeNullOrEmpty();
        currenciesFromResponse.Should().HaveCount(numberOfCurrencies);
        currenciesFromResponse.Should().AllSatisfy(c => currencyIds.Should().Contain(c.Id));
    }

    #endregion
    
    #region GET BY ID TESTS

    [Fact]
    public async Task GetById_ShouldReturnCurrencyWithCorrectId()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/currency/{currency.Id}");
        var currencyFromResponse = await response.Content.ReadFromJsonAsync<CurrencyDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        currencyFromResponse.Should().NotBeNull();
        currencyFromResponse.Id.Should().Be(currency.Id);
    }
    
    [Fact]
    public async Task GetById_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/currency/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/currency/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}