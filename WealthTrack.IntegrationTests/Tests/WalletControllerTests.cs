using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using WealthTrack.Data.Context;
using WealthTrack.IntegrationTests.Helpers;
using WealthTrack.Data.DomainModels;
using Microsoft.EntityFrameworkCore;
using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("WalletTests")]
    public class WalletControllerTests(SeededWebAppFactory factory) : IClassFixture<SeededWebAppFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client = factory.CreateClient();

        private IServiceScope _scope = null!;
        private AppDbContext _db = null!;

        private string balanceCorrectionId = factory.configuration["SystemCategories:BalanceCorrectionId"]!;

        public async Task InitializeAsync()
        {
            await factory.InitializeAsync();

            _scope = factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var currencyEntity = TestCurrencyModels.FirstDomainModelWithoutDetails;
            var categoryEntity = TestCategoryModels.FirstDomainModelWithoutDetails;
            var budgetEntity = TestBudgetModels.FirstDomainModelWithoutDetails;
            budgetEntity.CurrencyId = currencyEntity.Id;

            _db.Currencies.Add(currencyEntity);
            _db.Categories.Add(categoryEntity);
            _db.Budgets.Add(budgetEntity);

            await _db.SaveChangesAsync();
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
        public async Task GetAll_WithoutInclude_ReturnsAllWalletsWithoutRelatedEntities()
        {
            // Arrange
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            firstWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            secondWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var walletsToAdd = new List<Wallet>
            {
                firstWalletToAdd,
                secondWalletToAdd
            };

            var firstTransactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            firstTransactionToAdd.WalletId = firstWalletToAdd.Id;
            var secondTransactionToAdd = TestTransactionModels.SecondDomainModelWithoutDetails;
            secondTransactionToAdd.WalletId = secondWalletToAdd.Id;

            await _db.Wallets.AddRangeAsync(walletsToAdd);
            await _db.Transactions.AddRangeAsync([firstTransactionToAdd, secondTransactionToAdd]);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet");
            var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allWallets.Should().NotBeNullOrEmpty();
            allWallets.Should().HaveCount(walletsToAdd.Count);
            allWallets.Should().ContainSingle(wallet => wallet.Id == firstWalletToAdd.Id);
            allWallets.Should().ContainSingle(wallet => wallet.Id == secondWalletToAdd.Id);
            allWallets.Should().AllSatisfy(wallet => wallet.Currency.Should().BeNull());
            allWallets.Should().AllSatisfy(wallet => wallet.Budget.Should().BeNull());
            allWallets.Should().AllSatisfy(wallet => wallet.Transactions.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeCurrency_ReturnsAllWalletsWithCurrencyOnly()
        {
            // Arrange
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            firstWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            secondWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var walletsToAdd = new List<Wallet>
            {
                firstWalletToAdd,
                secondWalletToAdd
            };

            var firstTransactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            firstTransactionToAdd.WalletId = firstWalletToAdd.Id;
            var secondTransactionToAdd = TestTransactionModels.SecondDomainModelWithoutDetails;
            secondTransactionToAdd.WalletId = secondWalletToAdd.Id;

            await _db.Wallets.AddRangeAsync(walletsToAdd);
            await _db.Transactions.AddRangeAsync([firstTransactionToAdd, secondTransactionToAdd]);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet?include={nameof(Wallet.Currency)}");
            var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allWallets.Should().NotBeNullOrEmpty();
            allWallets.Should().HaveCount(walletsToAdd.Count);
            allWallets.Should().ContainSingle(wallet => wallet.Id == firstWalletToAdd.Id);
            allWallets.Should().ContainSingle(wallet => wallet.Id == secondWalletToAdd.Id);
            allWallets.Should().AllSatisfy(wallet => wallet.Currency.Should().NotBeNull());
            allWallets.Should().AllSatisfy(wallet => wallet.Budget.Should().BeNull());
            allWallets.Should().AllSatisfy(wallet => wallet.Transactions.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsAllWalletsWithAllRelatedEntities()
        {
            // Arrange
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            firstWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            secondWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var walletsToAdd = new List<Wallet>
            {
                firstWalletToAdd,
                secondWalletToAdd
            };

            var firstTransactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            firstTransactionToAdd.WalletId = firstWalletToAdd.Id;
            var secondTransactionToAdd = TestTransactionModels.SecondDomainModelWithoutDetails;
            secondTransactionToAdd.WalletId = secondWalletToAdd.Id;

            await _db.Wallets.AddRangeAsync(walletsToAdd);
            await _db.Transactions.AddRangeAsync([firstTransactionToAdd, secondTransactionToAdd]);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet?include={nameof(Wallet.Currency)},{nameof(Wallet.Budget)},{nameof(Wallet.Transactions)}");
            var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allWallets.Should().NotBeNullOrEmpty();
            allWallets.Should().HaveCount(walletsToAdd.Count);
            allWallets.Should().ContainSingle(wallet => wallet.Id == firstWalletToAdd.Id);
            allWallets.Should().ContainSingle(wallet => wallet.Id == secondWalletToAdd.Id);
            allWallets.Should().AllSatisfy(wallet => wallet.Currency.Should().NotBeNull());
            allWallets.Should().AllSatisfy(wallet => wallet.Budget.Should().NotBeNull());
            allWallets.Should().AllSatisfy(wallet => wallet.Transactions.Should().NotBeNullOrEmpty());
        }

        // GET BY ID tests

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsWalletWithoutRelatedEntities()
        {
            // Arrange
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var transactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            transactionToAdd.WalletId = walletToAdd.Id;

            await _db.Wallets.AddAsync(walletToAdd);
            await _db.Transactions.AddAsync(transactionToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet/{walletToAdd.Id}");
            var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            wallet.Should().NotBeNull();
            wallet.Id.Should().Be(walletToAdd.Id);
            wallet.Currency.Should().BeNull();
            wallet.Budget.Should().BeNull();
            wallet.Transactions.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeCurrency_ReturnsWalletWithCurrencyOnly()
        {
            // Arrange
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var transactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            transactionToAdd.WalletId = walletToAdd.Id;

            await _db.Wallets.AddAsync(walletToAdd);
            await _db.Transactions.AddAsync(transactionToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet/{walletToAdd.Id}?include={nameof(Wallet.Currency)}");
            var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            wallet.Should().NotBeNull();
            wallet.Id.Should().Be(walletToAdd.Id);
            wallet.Currency.Should().NotBeNull();
            wallet.Currency.Id.Should().Be(walletToAdd.CurrencyId);
            wallet.Budget.Should().BeNull();
            wallet.Transactions.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludedAllRelatedEntities_ReturnsWalletWithAllRelatedEntities()
        {
            // Arrange
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var transactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            transactionToAdd.WalletId = walletToAdd.Id;

            await _db.Wallets.AddAsync(walletToAdd);
            await _db.Transactions.AddAsync(transactionToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet/{walletToAdd.Id}?include={nameof(Wallet.Currency)},{nameof(Wallet.Budget)},{nameof(Wallet.Transactions)}");
            var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            wallet.Should().NotBeNull();
            wallet.Id.Should().Be(walletToAdd.Id);
            wallet.Currency.Should().NotBeNull();
            wallet.Currency.Id.Should().Be(walletToAdd.CurrencyId);
            wallet.Budget.Should().NotBeNull();
            wallet.Budget.Id.Should().Be(walletToAdd.BudgetId);
            wallet.Transactions.Should().NotBeNullOrEmpty();
            wallet.Transactions.Should().OnlyContain(transaction => transaction.Id == transactionToAdd.Id);
        }

        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFoundResult()
        {
            // Arrange
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;

            var transactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            transactionToAdd.WalletId = walletToAdd.Id;

            await _db.Wallets.AddAsync(walletToAdd);
            await _db.Transactions.AddAsync(transactionToAdd);
            await _db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/wallet/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // CREATE tests

        [Fact]
        public async Task Create_CreatesWallet()
        {
            // Arrange
            var walletToCreate = TestWalletModels.UpsertApiModel;
            walletToCreate.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToCreate.BudgetId = TestBudgetModels.FirstBudgetId;

            // Act
            var response = await _client.PostAsJsonAsync("/api/wallet/create", walletToCreate);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var createdWallet = await _db.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == createdId);
            createdWallet.Should().NotBeNull();
            createdWallet.Name.Should().Be(walletToCreate.Name);
            createdWallet.CurrencyId.Should().Be(walletToCreate.CurrencyId.Value);
            createdWallet.BudgetId.Should().Be(walletToCreate.BudgetId.Value);
        }

        [Fact]
        public async Task Create_CreatesWalletAsPartOfGeneralBalance_UpdatesBudgetBalance()
        {
            // Arrange
            var balanceValue = 150.50M;
            var walletToCreate = TestWalletModels.UpsertApiModel;
            walletToCreate.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToCreate.BudgetId = TestBudgetModels.FirstBudgetId;
            walletToCreate.IsPartOfGeneralBalance = true;
            walletToCreate.Balance = balanceValue;

            // Act
            var response = await _client.PostAsJsonAsync("/api/wallet/create", walletToCreate);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var budgetEntity = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == walletToCreate.BudgetId);
            budgetEntity.Should().NotBeNull();
            budgetEntity.OverallBalance.Should().Be(balanceValue);
        }

        [Fact]
        public async Task Create_CreatesWalletAsNotPartOfGeneralBalance_BudgetBalanceNotUpdated()
        {
            // Arrange
            var balanceValue = 150.50M;
            var walletToCreate = TestWalletModels.UpsertApiModel;
            walletToCreate.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToCreate.BudgetId = TestBudgetModels.FirstBudgetId;
            walletToCreate.IsPartOfGeneralBalance = false;
            walletToCreate.Balance = balanceValue;

            // Act
            var response = await _client.PostAsJsonAsync("/api/wallet/create", walletToCreate);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var budgetEntity = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == walletToCreate.BudgetId);
            budgetEntity.Should().NotBeNull();
            budgetEntity.OverallBalance.Should().Be(0);
        }

        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/wallet/create", (WalletUpsertApiModel)null!);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // UPDATE tests

        [Fact]
        public async Task Update_UpdatesWalletName()
        {
            // Arrange
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;
            await _db.Wallets.AddAsync(walletToAdd);
            await _db.SaveChangesAsync();

            var modelToUpdate = new WalletUpsertApiModel
            {
                Name = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/wallet/update/{walletToAdd.Id}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedWallet = await _db.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == walletToAdd.Id);
            updatedWallet.Should().NotBeNull();
            updatedWallet.Name.Should().Be(modelToUpdate.Name);
        }

        [Fact]
        public async Task Update_UpdateWalletBalanceWithBiggerAmount_ShouldCreateIncomeCorrectionTransaction()
        {
            // Arrange
            var initialBalance = 50M;
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;
            walletToAdd.Balance = initialBalance;
            await _db.Wallets.AddAsync(walletToAdd);
            await _db.SaveChangesAsync();
            var modelToUpdate = new WalletUpsertApiModel
            {
                Balance = 100
            };
            var expectedTransactionAmount = modelToUpdate.Balance - initialBalance;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/wallet/update/{walletToAdd.Id}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var allTransactions = await _db.Transactions.AsNoTracking().ToListAsync();
            allTransactions.Should().HaveCount(1);
            allTransactions.Should().AllSatisfy(transaction => transaction.CategoryId.ToString().Should().Be(balanceCorrectionId));
            allTransactions.Should().AllSatisfy(transaction => transaction.Amount.Should().Be(expectedTransactionAmount));
            allTransactions.Should().AllSatisfy(transaction => transaction.Type.Should().Be(TransactionType.Income));
        }

        [Fact]
        public async Task Update_UpdateWalletBalanceWithSmallerAmount_ShouldCreateExpenseCorrectionTransaction()
        {
            // Arrange
            var initialBalance = 50M;
            var walletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            walletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            walletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;
            walletToAdd.Balance = initialBalance;
            await _db.Wallets.AddAsync(walletToAdd);
            await _db.SaveChangesAsync();
            var modelToUpdate = new WalletUpsertApiModel
            {
                Balance = 25
            };
            var expectedTransactionAmount = initialBalance - modelToUpdate.Balance;

            // Act
            var response = await _client.PutAsJsonAsync($"/api/wallet/update/{walletToAdd.Id}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var allTransactions = await _db.Transactions.AsNoTracking().ToListAsync();
            allTransactions.Should().HaveCount(1);
            allTransactions.Should().AllSatisfy(transaction => transaction.CategoryId.ToString().Should().Be(balanceCorrectionId));
            allTransactions.Should().AllSatisfy(transaction => transaction.Amount.Should().Be(expectedTransactionAmount));
            allTransactions.Should().AllSatisfy(transaction => transaction.Type.Should().Be(TransactionType.Expense));
        }

        [Fact]
        public async Task Update_UpdateWalletBalance_WhenWalletIsPartOfGeneralBalanc_ShouldUpdateBudgetBalance()
        {

        }

        [Fact]
        public async Task Update_UpdateWalletBalance_WhenWalletIsNotPartOfGeneralBalanc_ShouldNotUpdateBudgetBalance()
        {

        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFoundResult()
        {
            // Arrange
            var modelToUpdate = new WalletUpsertApiModel
            {
                Name = Guid.NewGuid().ToString()
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/wallet/update/{Guid.NewGuid()}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // DELETE tests

        [Fact]
        public async Task HardDelete_DeletesWalletAndRelatedTransactions()
        {
            // Arrange
            var firstWalletToAdd = TestWalletModels.FirstDomainModelWithoutDetails;
            firstWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            firstWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;
            var secondWalletToAdd = TestWalletModels.SecondDomainModelWithoutDetails;
            secondWalletToAdd.CurrencyId = TestCurrencyModels.FirstCurrencyId;
            secondWalletToAdd.BudgetId = TestBudgetModels.FirstBudgetId;
            var transactionToAdd = TestTransactionModels.FirstDomainModelWithoutDetails;
            transactionToAdd.WalletId = firstWalletToAdd.Id;
            var transferTransactionToAdd = TestTransferTransactionModels.FirstDomainModelWithoutDetails;
            transferTransactionToAdd.SourceWalletId = firstWalletToAdd.Id;
            transferTransactionToAdd.TargetWalletId = secondWalletToAdd.Id;

            await _db.Wallets.AddAsync(firstWalletToAdd);
            await _db.Wallets.AddAsync(secondWalletToAdd);
            await _db.Transactions.AddAsync(transactionToAdd);
            await _db.TransferTransactions.AddAsync(transferTransactionToAdd);
            await _db.SaveChangesAsync();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/wallet/hard_delete/{firstWalletToAdd.Id}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var deletedWallet = await _db.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == firstWalletToAdd.Id);
            var deletedTransaction = await _db.Transactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == transactionToAdd.Id);
            var deletedTransferTransactions = await _db.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == transferTransactionToAdd.Id);
            var existedCurrency = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == TestCurrencyModels.FirstCurrencyId);
            var existedBudget = await _db.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == TestBudgetModels.FirstBudgetId);

            deletedWallet.Should().BeNull();
            deletedTransaction.Should().BeNull();
            deletedTransferTransactions.Should().BeNull();
            existedCurrency.Should().NotBeNull();
            existedBudget.Should().NotBeNull();
        }

        [Fact]
        public async Task HardDelete_WhenWalletIsPartOfGeneralBalance_BudgetBalanceUpdated()
        {

        }

        [Fact]
        public async Task HardDelete_WhenWalletIsNotPartOfGeneralBalance_BudgetBalanceNotUpdated()
        {

        }
    }
}
