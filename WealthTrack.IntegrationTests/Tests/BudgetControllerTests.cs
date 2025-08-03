using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.Data.Context;
using WealthTrack.IntegrationTests.WebAppFactories;
using WealthTrack.IntegrationTests.Helpers;
using WealthTrack.Data.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("BudgetTests")]
    public class BudgetControllerTests : IClassFixture<EmptyWebAppFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly EmptyWebAppFactory _factory;

        private IServiceScope _scope = null!;
        private AppDbContext _db = null!;

        public BudgetControllerTests(EmptyWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await _factory.InitializeAsync();

            _scope = _factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var currencyEntity = TestCurrencyModels.DomainModelWithoutDetails;

            var budgetEntity = TestBudgetModels.DomainModelWithoutDetails;
            budgetEntity.CurrencyId = TestCurrencyModels.CurrencyId;

            var walletEntity = TestWalletModels.DomainModelWithoutDetails;
            walletEntity.CurrencyId = TestCurrencyModels.CurrencyId;
            walletEntity.BudgetId = TestBudgetModels.BudgetId;

            _db.Currencies.Add(currencyEntity);
            _db.Budgets.Add(budgetEntity);
            _db.Wallets.Add(walletEntity);

            await _db.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            if (_db != null)
            {
                _db.Wallets.RemoveRange(_db.Wallets);
                _db.Budgets.RemoveRange(_db.Budgets);
                _db.Currencies.RemoveRange(_db.Currencies);
                await _db.SaveChangesAsync();
            }

            _scope?.Dispose();
        }

        // GET ALL tests

        [Fact]
        public async Task GetAll_WithoutInclude_ReturnsAllBudgetsWithoutRelatedEntities()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().ContainSingle(budget => budget.Id == TestBudgetModels.BudgetId);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().BeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeCurrency_ReturnsAllBudgetsWithCurrency()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget?include={nameof(Budget.Currency)}");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().ContainSingle(budget => budget.Id == TestBudgetModels.BudgetId);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeCurrencyAndWallets_ReturnsAllBudgetsWithCurrencyAndWallets()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().ContainSingle(budget => budget.Id == TestBudgetModels.BudgetId);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().NotBeEmpty());
        }


        // GET BY ID tests

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsBudgetWithoutRelatedEntities()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget/{TestBudgetModels.BudgetId}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget!.Id.Should().Be(TestBudgetModels.BudgetId);
            budget.Currency.Should().BeNull();
            budget.Wallets.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeCurrency_ReturnsBudgetWithCurrency()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget/{TestBudgetModels.BudgetId}?include={nameof(Budget.Currency)}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget!.Id.Should().Be(TestBudgetModels.BudgetId);
            budget.Currency.Should().NotBeNull();
            budget.Currency.Id.Should().Be(TestCurrencyModels.CurrencyId);
            budget.Wallets.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeCurrencyAndWallets_ReturnsBudgetWithCurrencyAndWallets()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget/{TestBudgetModels.BudgetId}?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget!.Id.Should().Be(TestBudgetModels.BudgetId);
            budget.Currency.Should().NotBeNull();
            budget.Currency.Id.Should().Be(TestCurrencyModels.CurrencyId);
            budget.Wallets.Should().NotBeEmpty();
            budget.Wallets.Should().OnlyContain(wallet => wallet.Id == TestWalletModels.WalletId);
        }

        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFoundResult()
        {
            // Act
            var response = await _client.GetAsync($"/api/budget/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // CREATE tests

        [Fact]
        public async Task Create_WithCurrency_CreatesBudgetWithRelatedEntities()
        {
            // Arrange
            var budgetToCreate = new BudgetUpsertApiModel
            {
                Name = Guid.NewGuid().ToString(),
                CurrencyId = TestCurrencyModels.CurrencyId
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/budget/create", budgetToCreate);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var createdBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == createdId);
            createdBudget.Should().NotBeNull();
            createdBudget.Name.Should().Be(budgetToCreate.Name);
            createdBudget.CurrencyId.Should().Be(TestCurrencyModels.CurrencyId);
        }

        [Fact]
        public async Task Create_WithNullBodyRequest_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/budget/create", (BudgetUpsertApiModel)null!);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // UPDATE tests

        [Fact]
        public async Task Update_UpdatesBudgetName()
        {
            // Arrange
            var modelToUpdate = new BudgetUpsertApiModel
            {
                Name = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/budget/update/{TestBudgetModels.BudgetId}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == TestBudgetModels.BudgetId);
            updatedBudget.Should().NotBeNull();
            updatedBudget.Name.Should().Be(modelToUpdate.Name);
        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFoundResult()
        {
            // Arrange
            var modelToUpdate = new BudgetUpsertApiModel
            {
                Name = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/budget/update/{Guid.NewGuid()}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // DELETE tests

        [Fact]
        public async Task HardDelete_DeletesBudget()
        {
            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/budget/hard_delete/{TestBudgetModels.BudgetId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var deletedBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == TestBudgetModels.BudgetId);
            var deletedWallet = await _db.Wallets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == TestWalletModels.WalletId);
            deletedBudget.Should().BeNull();
            deletedWallet.Should().BeNull();
        }
    }
}
