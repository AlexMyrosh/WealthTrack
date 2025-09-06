using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using WealthTrack.Data.Context;
using WealthTrack.IntegrationTests.WebAppFactories;
using WealthTrack.IntegrationTests.Helpers;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("CurrencyTests")]
    public class CurrencyControllerTests(EmptyWebAppFactory factory) : IClassFixture<EmptyWebAppFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client = factory.CreateClient();

        private IServiceScope _scope = null!;
        private AppDbContext _db = null!;

        public async Task InitializeAsync()
        {
            await factory.InitializeAsync();

            _scope = factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        public async Task DisposeAsync()
        {
            _scope?.Dispose();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Goals.RemoveRange(db.Goals);
            db.Currencies.RemoveRange(db.Currencies);
            db.Categories.RemoveRange(db.Categories);
            db.Budgets.RemoveRange(db.Budgets);
            db.Wallets.RemoveRange(db.Wallets);
            db.Transactions.RemoveRange(db.Transactions);
            db.TransferTransactions.RemoveRange(db.TransferTransactions);

            await db.SaveChangesAsync();
        }

        // GET ALL tests

        [Fact]
        public async Task GetAll_ReturnsAllCurrencies()
        {
            // Arrange
            var currenciesToAdd = new List<Currency>
            {
                TestCurrencyModels.FirstDomainModelWithoutDetails,
                TestCurrencyModels.SecondDomainModelWithoutDetails
            };
            await _db.Currencies.AddRangeAsync(currenciesToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/currency");
            var allCurrencies = await response.Content.ReadFromJsonAsync<List<CurrencyDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allCurrencies.Should().NotBeNullOrEmpty();
            allCurrencies.Should().HaveCount(currenciesToAdd.Count);
            allCurrencies.Should().ContainSingle(currency => currency.Id == TestCurrencyModels.FirstCurrencyId);
            allCurrencies.Should().ContainSingle(currency => currency.Id == TestCurrencyModels.SecondCurrencyId);
        }

        // GET BY ID tests

        [Fact]
        public async Task GetById_ReturnsCurrency()
        {
            // Arrange
            var currencyToAdd = TestCurrencyModels.FirstDomainModelWithoutDetails;
            await _db.Currencies.AddAsync(currencyToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/currency/{currencyToAdd.Id}");
            var currency = await response.Content.ReadFromJsonAsync<CurrencyDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            currency.Should().NotBeNull();
            currency.Id.Should().Be(currencyToAdd.Id);
        }

        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFoundResult()
        {
            // Arrange
            var currencyToAdd = TestCurrencyModels.FirstDomainModelWithoutDetails;
            await _db.Currencies.AddAsync(currencyToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/currency/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
