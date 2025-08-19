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
    public class BudgetControllerTests(EmptyWebAppFactory factory) : IClassFixture<EmptyWebAppFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly EmptyWebAppFactory _factory = factory;

        private IServiceScope _scope = null!;
        private AppDbContext _db = null!;

        public async Task InitializeAsync()
        {
            await _factory.InitializeAsync();

            _scope = _factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var currencyEntity = TestCurrencyModels.FirstDomainModelWithoutDetails;
            _db.Currencies.Add(currencyEntity);

            await _db.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            _scope?.Dispose();

            using var scope = _factory.Services.CreateScope();
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
        public async Task GetAll_WithoutInclude_ReturnsAllBudgetsWithoutRelatedEntities()
        {
            // Arrange
            var firstBudgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            firstBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondBudgetToAdd = TestBudgetModels.SecondDomainModelWithoutDetails;
            secondBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var budgetsToAdd = new List<Budget>
            {
                firstBudgetToAdd,
                secondBudgetToAdd
            };
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.BudgetId = firstBudgetToAdd.Id;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.BudgetId = secondBudgetToAdd.Id;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;

            await _db.Budgets.AddRangeAsync(budgetsToAdd);
            await _db.Wallets.AddRangeAsync([firstWalletToAdd, secondWalletToAdd]);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/budget");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().HaveCount(budgetsToAdd.Count);
            allBudgets.Should().ContainSingle(budget => budget.Id == firstBudgetToAdd.Id);
            allBudgets.Should().ContainSingle(budget => budget.Id == secondBudgetToAdd.Id);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().BeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeCurrency_ReturnsAllBudgetsWithCurrencyOnly()
        {
            // Arrange
            var firstBudgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            firstBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondBudgetToAdd = TestBudgetModels.SecondDomainModelWithoutDetails;
            secondBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var budgetsToAdd = new List<Budget>
            {
                firstBudgetToAdd,
                secondBudgetToAdd
            };
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.BudgetId = firstBudgetToAdd.Id;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.BudgetId = secondBudgetToAdd.Id;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;

            await _db.Budgets.AddRangeAsync(budgetsToAdd);
            await _db.Wallets.AddRangeAsync([firstWalletToAdd, secondWalletToAdd]);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/budget?include={nameof(Budget.Currency)}");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().HaveCount(budgetsToAdd.Count);
            allBudgets.Should().ContainSingle(budget => budget.Id == firstBudgetToAdd.Id);
            allBudgets.Should().ContainSingle(budget => budget.Id == secondBudgetToAdd.Id);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsAllBudgetsWithAllRelatedEntities()
        {
            // Arrange
            var firstBudgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            firstBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondBudgetToAdd = TestBudgetModels.SecondDomainModelWithoutDetails;
            secondBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var budgetsToAdd = new List<Budget>
            {
                firstBudgetToAdd,
                secondBudgetToAdd
            };
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.BudgetId = firstBudgetToAdd.Id;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.BudgetId = secondBudgetToAdd.Id;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;

            await _db.Budgets.AddRangeAsync(budgetsToAdd);
            await _db.Wallets.AddRangeAsync([firstWalletToAdd, secondWalletToAdd]);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/budget?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().HaveCount(budgetsToAdd.Count);
            allBudgets.Should().ContainSingle(budget => budget.Id == firstBudgetToAdd.Id);
            allBudgets.Should().ContainSingle(budget => budget.Id == secondBudgetToAdd.Id);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().NotBeNullOrEmpty());
        }

        // GET BY ID tests

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsBudgetWithoutRelatedEntities()
        {
            // Arrange
            var budgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            budgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.BudgetId = budgetToAdd.Id;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;

            await _db.Budgets.AddAsync(budgetToAdd);
            await _db.Wallets.AddAsync(walletToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/budget/{budgetToAdd.Id}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget.Id.Should().Be(budgetToAdd.Id);
            budget.Currency.Should().BeNull();
            budget.Wallets.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeCurrency_ReturnsBudgetWithCurrencyOnly()
        {
            // Arrange
            var budgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            budgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.BudgetId = budgetToAdd.Id;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;

            await _db.Budgets.AddAsync(budgetToAdd);
            await _db.Wallets.AddAsync(walletToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/budget/{budgetToAdd.Id}?include={nameof(Budget.Currency)}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget.Id.Should().Be(budgetToAdd.Id);
            budget.Currency.Should().NotBeNull();
            budget.Currency.Id.Should().Be(budgetToAdd.CurrencyId);
            budget.Wallets.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeAllRelatedEntities_ReturnsBudgetWithAllRelatedEntities()
        {
            // Arrange
            var budgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            budgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.BudgetId = budgetToAdd.Id;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;

            await _db.Budgets.AddAsync(budgetToAdd);
            await _db.Wallets.AddAsync(walletToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/budget/{budgetToAdd.Id}?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget.Id.Should().Be(budgetToAdd.Id);
            budget.Currency.Should().NotBeNull();
            budget.Currency.Id.Should().Be(budgetToAdd.CurrencyId);
            budget.Wallets.Should().NotBeEmpty();
            budget.Wallets.Should().OnlyContain(wallet => wallet.Id == walletToAdd.Id);
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
        public async Task Create_CreatesBudget()
        {
            // Arrange
            var budgetToCreate = new BudgetUpsertApiModel
            {
                Name = Guid.NewGuid().ToString(),
                CurrencyId = TestCurrencyModels.FirstCurrencyId
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
            createdBudget.CurrencyId.Should().Be(budgetToCreate.CurrencyId!.Value);
        }

        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
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
            var budgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            budgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            await _db.Budgets.AddAsync(budgetToAdd);
            await _db.SaveChangesAsync();
            var modelToUpdate = new BudgetUpsertApiModel
            {
                Name = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/budget/update/{budgetToAdd.Id}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == budgetToAdd.Id);
            updatedBudget.Should().NotBeNull();
            updatedBudget.Name.Should().Be(modelToUpdate.Name);
            updatedBudget.CurrencyId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFoundResult()
        {
            // Arrange
            var budgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            budgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            await _db.Budgets.AddAsync(budgetToAdd);
            await _db.SaveChangesAsync();
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
        public async Task HardDelete_DeletesBudgetAndAllRelatedData()
        {
            // Arrange
            var firstBudgetToAdd = TestBudgetModels.FirstDomainModelWithoutDetails;
            firstBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondBudgetToAdd = TestBudgetModels.SecondDomainModelWithoutDetails;
            secondBudgetToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.BudgetId = firstBudgetToAdd.Id;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.BudgetId = secondBudgetToAdd.Id;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            var transactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            transactionToAdd.WalletId = firstWalletToAdd.Id;
            var firstTransferTransactionToAdd = TestTransferTransactionModels.FirstDomainModelWithoutDetails;
            firstTransferTransactionToAdd.SourceWalletId = firstWalletToAdd.Id;
            firstTransferTransactionToAdd.TargetWalletId = secondWalletToAdd.Id;
            var secondTransferTransactionToAdd = TestTransferTransactionModels.SecondDomainModelWithoutDetails;
            secondTransferTransactionToAdd.SourceWalletId = secondWalletToAdd.Id;
            secondTransferTransactionToAdd.TargetWalletId = firstWalletToAdd.Id;

            await _db.Budgets.AddAsync(firstBudgetToAdd);
            await _db.Budgets.AddAsync(secondBudgetToAdd);
            await _db.Wallets.AddAsync(firstWalletToAdd);
            await _db.Wallets.AddAsync(secondWalletToAdd);
            await _db.Transactions.AddAsync(transactionToAdd);
            await _db.TransferTransactions.AddAsync(firstTransferTransactionToAdd);
            await _db.TransferTransactions.AddAsync(secondTransferTransactionToAdd);
            await _db.SaveChangesAsync();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/budget/hard_delete/{firstBudgetToAdd.Id}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var deletedBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == firstBudgetToAdd.Id);
            var deletedFirstWallet = await _db.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == firstWalletToAdd.Id);
            var deletedTransaction = await _db.Transactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == transactionToAdd.Id);
            var deletedFirstTransferTransaction = await _db.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == firstTransferTransactionToAdd.Id);
            var deletedSecondTransferTransaction = await _db.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == secondTransferTransactionToAdd.Id);
            var existedBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == secondBudgetToAdd.Id);
            var existedSecondWallet = await _db.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == secondWalletToAdd.Id);
            var existedCurrency = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == TestCurrencyModels.FirstCurrencyId);

            deletedBudget.Should().BeNull();
            deletedFirstWallet.Should().BeNull();
            deletedTransaction.Should().BeNull();
            deletedFirstTransferTransaction.Should().BeNull();
            deletedSecondTransferTransaction.Should().BeNull();
            existedBudget.Should().NotBeNull();
            existedSecondWallet.Should().NotBeNull();
            existedCurrency.Should().NotBeNull();
        }
    }
}
