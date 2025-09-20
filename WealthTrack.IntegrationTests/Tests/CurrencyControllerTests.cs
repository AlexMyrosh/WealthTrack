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
    // GET ALL tests
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_ReturnsAllCurrencies(int numberOfBudgets)
    {
        // Arrange
        var currencies = DataFactory.CreateCurrenciesScenario(numberOfBudgets);
        DbContext.Currencies.AddRange(currencies);
        await DbContext.SaveChangesAsync();
        var currencyIds = currencies.Select(b => b.Id).ToList();
        
        // Act
        var response = await Client.GetAsync("/api/currency");
        var allCurrencies = await response.Content.ReadFromJsonAsync<List<CurrencyDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allCurrencies.Should().NotBeNullOrEmpty();
        allCurrencies.Should().HaveCount(numberOfBudgets);
        allCurrencies.Should().AllSatisfy(currency => currencyIds.Should().Contain(currency.Id));
    }

    // GET BY ID tests
    
    [Fact]
    public async Task GetById_ReturnsCurrency()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/currency/{currency.Id}");
        var model = await response.Content.ReadFromJsonAsync<CurrencyDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(currency.Id);
    }

    [Fact]
    public async Task GetById_WithWrongId_ReturnsNotFound()
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
}