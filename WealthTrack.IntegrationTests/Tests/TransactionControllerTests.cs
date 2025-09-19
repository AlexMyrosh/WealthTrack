using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Transaction;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("TransactionTests")]
public class TransactionControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
{
    // GET ALL tests
    
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ReturnsAllTransactionsWithoutRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/transaction");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }

    [Theory]
    [InlineData(1,1)]
    [InlineData(2,2)]
    [InlineData(3,3)]
    public async Task GetAll_ReturnsRegularAndTransferTransactionsTogether(int numberOfTransactions, int numberOfTransfers)
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(numberOfTransactions, numberOfTransfers);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var totalNumberOfTransactions = numberOfTransactions + numberOfTransfers;

        // Act
        var response = await Client.GetAsync("/api/transaction");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Count.Should().Be(totalNumberOfTransactions);
    }
    
    [Fact]
    public async Task GetAll_RegularTransactions_WithIncludedCategory_ReturnsTransactionsWithCategoryOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var regularTransactionIds = scenario.transactions.Select(t => t.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/transaction?include={nameof(Transaction.Category)}");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        allTransactions.Where(t => regularTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().NotBeNull());
    }
    
    [Fact]
    public async Task GetAll_RegularTransactions_WithIncludedWallet_ReturnsAllTransactionsWithWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var regularTransactionIds = scenario.transactions.Select(t => t.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/transaction?include={nameof(Transaction.Wallet)}");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Where(t => regularTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Wallet.Should().NotBeNull());
        allTransactions.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_TransferTransactions_WithIncludedSourceWallet_ReturnsAllTransfersWithSourceWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var transferTransactionIds = scenario.transferTransactions.Select(t => t.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/transaction?include={nameof(TransferTransaction.SourceWallet)}");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        allTransactions.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.SourceWallet.Should().NotBeNull());
        allTransactions.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_TransferTransactions_WithIncludedTargetWallet_ReturnsAllTransfersWithTargetWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var transferTransactionIds = scenario.transferTransactions.Select(t => t.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/transaction?include={nameof(TransferTransaction.TargetWallet)}");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        allTransactions.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.TargetWallet.Should().NotBeNull());
        allTransactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsAllTransactionsWithAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var transactionIds = scenario.transactions.Select(t => t.Id).ToList();
        var transferTransactionIds = scenario.transferTransactions.Select(t => t.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/transaction?include={nameof(Transaction.Wallet)}," +
                                             $"{nameof(Transaction.Category)}," +
                                             $"{nameof(TransferTransaction.SourceWallet)}," +
                                             $"{nameof(TransferTransaction.TargetWallet)}");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Where(t => transactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Wallet.Should().NotBeNull());
        allTransactions.Where(t => transactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().NotBeNull());
        allTransactions.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.SourceWallet.Should().NotBeNull());
        allTransactions.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.TargetWallet.Should().NotBeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ReturnsAllTransactionsWithoutRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(2, 2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/transaction?include=SomeProperty");
        var allTransactions = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allTransactions.Should().NotBeNullOrEmpty();
        allTransactions.Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        allTransactions.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }

    // GET BY ID tests
    
    [Fact]
    public async Task GetById_RegularTransaction_WithoutIncludeParameter_ReturnsTransactionWithoutRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().BeNull();
        model.Category.Should().BeNull();
        model.SourceWallet.Should().BeNull();
        model.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_RegularTransaction_WithIncludedCategory_ReturnsTransactionWithCategoryOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include={nameof(Transaction.Category)}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().BeNull();
        model.Category.Should().NotBeNull();
        model.SourceWallet.Should().BeNull();
        model.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_RegularTransaction_WithIncludedWallet_ReturnsTransactionWithWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include={nameof(Transaction.Wallet)}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().NotBeNull();
        model.Category.Should().BeNull();
        model.SourceWallet.Should().BeNull();
        model.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_RegularTransaction_WithIncludedCategoryAndWallet_ReturnsTransactionWithCategoryAndWallet()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include={nameof(Transaction.Wallet)},{nameof(Transaction.Category)}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().NotBeNull();
        model.Category.Should().NotBeNull();
        model.SourceWallet.Should().BeNull();
        model.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_TransferTransactions_WithIncludedSourceWallet_ReturnsTransferWithSourceWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include={nameof(TransferTransaction.SourceWallet)}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().BeNull();
        model.Category.Should().BeNull();
        model.SourceWallet.Should().NotBeNull();
        model.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_TransferTransactions_WithIncludedTargetWallet_ReturnsTransferWithTargetWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include={nameof(TransferTransaction.TargetWallet)}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().BeNull();
        model.Category.Should().BeNull();
        model.SourceWallet.Should().BeNull();
        model.TargetWallet.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetById_TransferTransactions_WithIncludedSourceAndTargetWallets_ReturnsTransferWithSourceAndTargetWallets()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include={nameof(TransferTransaction.TargetWallet)},{nameof(TransferTransaction.SourceWallet)}");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().BeNull();
        model.Category.Should().BeNull();
        model.SourceWallet.Should().NotBeNull();
        model.TargetWallet.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ReturnsTransactionWithoutRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include=SomeProperty");
        var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        model.Should().NotBeNull();
        model.Id.Should().Be(scenario.transaction.Id);
        model.Wallet.Should().BeNull();
        model.Category.Should().BeNull();
        model.SourceWallet.Should().BeNull();
        model.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/transaction/{Guid.NewGuid()}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetById_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/transaction/{Guid.Empty}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // CREATE REGULAR TRANSACTION tests
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Create_RegularTransaction_WithCorrectData_CreatesNewTransactionWithCorrectDefaultData(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory(c => c.Type = type);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var created = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == createdId);
        created.Should().NotBeNull();
        created.Amount.Should().Be(upsert.Amount);
        created.Description.Should().Be(upsert.Description);
        created.TransactionDate.Should().Be(upsert.TransactionDate);
        created.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        created.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        created.Type.Should().Be(upsert.Type);
        created.CategoryId.Should().Be(upsert.CategoryId);
        created.WalletId.Should().Be(upsert.WalletId.Value);
    }

    [Fact]
    public async Task Create_RegularTransaction_IncomeType_WalletBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory(c => c.Type = OperationType.Income);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = OperationType.Income,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.wallet.Balance + upsert.Amount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedBalanceAmount);
    }

    [Fact]
    public async Task Create_RegularTransaction_ExpenseType_WalletBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory(c => c.Type = OperationType.Expense);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = OperationType.Expense,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.wallet.Balance - upsert.Amount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedBalanceAmount);
    }

    [Fact]
    public async Task Create_RegularTransaction_IncomeType_WhenWalletIsPartOfGeneralBalance_BudgetBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario(configureWallet: w => w.IsPartOfGeneralBalance = true);
        var category = DataFactory.CreateCategory(c => c.Type = OperationType.Income);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = OperationType.Income,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.budget.OverallBalance + upsert.Amount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(expectedBalanceAmount);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_ExpenseType_WhenWalletIsPartOfGeneralBalance_BudgetBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario(configureWallet: w => w.IsPartOfGeneralBalance = true);
        var category = DataFactory.CreateCategory(c => c.Type = OperationType.Expense);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = OperationType.Expense,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.budget.OverallBalance - upsert.Amount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(expectedBalanceAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_RegularTransaction_WhenWalletIsNotPartOfGeneralBalance_BudgetBalanceIsNotUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario(configureWallet: w => w.IsPartOfGeneralBalance = false);
        var category = DataFactory.CreateCategory(c => c.Type = type);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.budget.OverallBalance;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(expectedBalanceAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_RegularTransaction_TransactionThatMeetsGoalConditions_GoalActualMoneyAmountIsUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedActualMoneyAmount = goal.ActualMoneyAmount + upsert.Amount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(t => t.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_RegularTransaction_TransactionThatDoesntMeetsGoalConditions_GoalActualMoneyAmountIsNotUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(1);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedActualMoneyAmount = goal.ActualMoneyAmount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(t => t.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
        [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_RegularTransaction_WithCategoryOfAnotherType_ReturnsBandRequest(OperationType transactionType, OperationType categoryType)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory(c => c.Type = categoryType);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = transactionType,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_RegularTransaction_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", (TransactionUpsertApiModel)null!);
        
        // Arrange
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(-1.0)]
    [InlineData(null)]
    public async Task Create_RegularTransaction_WithIncorrectAmountValue_ReturnsBadRequest(double? amountValue)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = (decimal?)amountValue!,
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = category.Type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData((OperationType)99)]
    [InlineData(null)]
    public async Task Create_RegularTransaction_WithIncorrectType_ReturnsBadRequest(OperationType? type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithNullTransactionDate_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = null,
            Type = category.Type,
            CategoryId = category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithIncorrectCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = category.Type,
            CategoryId = Guid.NewGuid(),
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithEmptyCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = category.Type,
            CategoryId = Guid.Empty,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithIncorrectWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = category.Type,
            CategoryId = category.Id,
            WalletId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithEmptyWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = category.Type,
            CategoryId = category.Id,
            WalletId = Guid.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithNullWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = category.Type,
            CategoryId = category.Id,
            WalletId = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // CREATE TRANSFER TRANSACTION tests
    
    [Fact]
    public async Task Create_TransferTransactionWithCorrectData_CreatesNewTransactionWithCorrectDefaultData()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet1);
        DbContext.Wallets.Add(scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = scenario.wallet2.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var createdTransfer = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == createdId);
        createdTransfer.Should().NotBeNull();
        createdTransfer.Amount.Should().Be(upsert.Amount);
        createdTransfer.Description.Should().Be(upsert.Description);
        createdTransfer.TransactionDate.Should().Be(upsert.TransactionDate);
        createdTransfer.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdTransfer.ModifiedDate.Should().BeSameDateAs(createdTransfer.CreatedDate);
        createdTransfer.SourceWalletId.Should().Be(upsert.SourceWalletId.Value);
        createdTransfer.TargetWalletId.Should().Be(upsert.TargetWalletId.Value);
    }

    [Fact]
    public async Task Create_TransferTransaction_SourceAndTargetWalletBalancesAreUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = scenario.wallet2.Id
        };
        
        var expectedSourceBalance = scenario.wallet1.Balance - upsert.Amount;
        var expectedTargetBalance = scenario.wallet2.Balance + upsert.Amount;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == upsert.SourceWalletId);
        var targetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == upsert.TargetWalletId);
        sourceWallet.Should().NotBeNull();
        targetWallet.Should().NotBeNull();
        sourceWallet.Balance.Should().Be(expectedSourceBalance);
        targetWallet.Balance.Should().Be(expectedTargetBalance);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_BetweenWalletsOfDifferentBudgets_ReturnsBadRequest()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        var budget1 = DataFactory.CreateBudget(b => b.CurrencyId = currency.Id);
        var budget2 = DataFactory.CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet1 = DataFactory.CreateWallet(w =>
        {
            w.CurrencyId = currency.Id;
            w.BudgetId = budget1.Id;
        });
        
        var wallet2 = DataFactory.CreateWallet(w =>
        {
            w.CurrencyId = currency.Id;
            w.BudgetId = budget2.Id;
        });
        DbContext.Currencies.Add(currency);
        DbContext.Budgets.AddRange(budget1, budget2);
        DbContext.Wallets.AddRange(wallet1, wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = wallet1.Id,
            TargetWalletId = wallet2.Id
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var category = DataFactory.CreateCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", (TransactionUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(-1.0)]
    [InlineData(null)]
    public async Task Create_TransferTransaction_WithIncorrectAmountValue_ReturnsBadRequest(double? amountValue)
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = (decimal?)amountValue,
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = scenario.wallet2.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithNullTransactionDate_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = null,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = scenario.wallet2.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithNullSourceWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = null,
            TargetWalletId = scenario.wallet2.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithIncorrectSourceWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = Guid.NewGuid(),
            TargetWalletId = scenario.wallet2.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithEmptySourceWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = Guid.Empty,
            TargetWalletId = scenario.wallet2.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithNullTargetWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithIncorrectTargetWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithEmptyTargetWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateTwoWalletsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1,  scenario.wallet2);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet1.Id,
            TargetWalletId = Guid.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // UPDATE REGULAR TRANSACTION tests
    
    [Fact]
    public async Task Update_RegularTransaction_WithNewAmount_UpdatesTransactionAmountOnly() 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(upsert.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().Be(scenario.transaction.TransactionDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithNewDescription_UpdatesTransactionDescriptionOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Description = $"Updated description + {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(upsert.Description);
        updatedTransaction.TransactionDate.Should().Be(scenario.transaction.TransactionDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithNewTransactionDate_UpdatesTransactionDateOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            TransactionDate = DateTimeOffset.UtcNow
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(upsert.TransactionDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_RegularTransaction_WithNewCategory_UpdatesTransactionCategoryOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
            );
        var newCategory = DataFactory.CreateCategory(c => c.Type = type);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category, newCategory);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            CategoryId = newCategory.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(upsert.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithNewWallet_UpdatesTransactionWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId =  scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet, newWallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            WalletId = newWallet.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(upsert.WalletId.Value);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_IncomeType_WithNewAmount_ShouldUpdateWalletBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
                configureCategory: c => c.Type = OperationType.Income,
                configureTransaction: t => t.Type = OperationType.Income
            );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        var expectedWalletBalance = scenario.wallet.Balance - scenario.transaction.Amount + upsert.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedWalletBalance);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_ExpenseType_WithNewAmount_ShouldUpdateWalletBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Expense,
            configureTransaction: t => t.Type = OperationType.Expense
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        var expectedWalletBalance = scenario.wallet.Balance + scenario.transaction.Amount - upsert.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedWalletBalance);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithNewAmount_IncomeType_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Income,
            configureTransaction: t => t.Type = OperationType.Income,
            configureWallet: w => w.IsPartOfGeneralBalance = true
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        var expectedBudgetBalance = scenario.budget.OverallBalance - scenario.transaction.Amount + upsert.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithNewAmount_ExpenseType_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Expense,
            configureTransaction: t => t.Type = OperationType.Expense,
            configureWallet: w => w.IsPartOfGeneralBalance = true
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        var expectedBudgetBalance = scenario.budget.OverallBalance + scenario.transaction.Amount - upsert.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewAmount_WhenWalletIsNotPartOfGeneralBalance_ShouldNotUpdateBudgetBalance(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type,
            configureWallet: w => w.IsPartOfGeneralBalance = false
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(scenario.budget.OverallBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransactionWithNewAmount_TransactionThatMeetsGoalConditions_GoalActualMoneyAmountIsUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
            );
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };

        var expectedActualMoneyAmount = goal.ActualMoneyAmount - scenario.transaction.Amount + upsert.Amount;
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransactionWithNewAmount_TransactionThatDoesntMeetsGoalConditions_GoalActualMoneyAmountIsNotUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
        );
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(1);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(goal.ActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_BecomesApplicableForGoal_ShouldUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t =>
            {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow.AddDays(-5);
            });
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-1);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            TransactionDate = DateTimeOffset.UtcNow
        };
        
        var expectedActualMoneyAmount = goal.ActualMoneyAmount + scenario.transaction.Amount;
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_BecomesNotApplicableForGoal_ShouldUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t =>
            {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-1);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            TransactionDate = DateTimeOffset.UtcNow.AddDays(-5)
        };
        
        var expectedActualMoneyAmount = goal.ActualMoneyAmount - scenario.transaction.Amount;
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_IncomeType_WithNewWallet_ShouldUpdateWalletsBalances()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Income,
            configureTransaction: t => t.Type = OperationType.Income
        );
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId =  scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet, newWallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            WalletId = newWallet.Id
        };

        var expectedOldWalletBalance = scenario.wallet.Balance - scenario.transaction.Amount;
        var expectedNewWalletBalance = newWallet.Balance + scenario.transaction.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedOldWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        var updatedNewWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == newWallet.Id);
        updatedOldWallet.Should().NotBeNull();
        updatedNewWallet.Should().NotBeNull();
        updatedOldWallet.Balance.Should().Be(expectedOldWalletBalance);
        updatedNewWallet.Balance.Should().Be(expectedNewWalletBalance);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_ExpenseType_WithNewWallet_ShouldUpdateWalletsBalances()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Expense,
            configureTransaction: t => t.Type = OperationType.Expense
        );
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId =  scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet, newWallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            WalletId = newWallet.Id
        };

        var expectedOldWalletBalance = scenario.wallet.Balance + scenario.transaction.Amount;
        var expectedNewWalletBalance = newWallet.Balance - scenario.transaction.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedOldWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        var updatedNewWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == newWallet.Id);
        updatedOldWallet.Should().NotBeNull();
        updatedNewWallet.Should().NotBeNull();
        updatedOldWallet.Balance.Should().Be(expectedOldWalletBalance);
        updatedNewWallet.Balance.Should().Be(expectedNewWalletBalance);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", (TransactionUpsertApiModel)null!);
        
        //Arrange
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(-1.0)]
    public async Task Update_RegularTransaction_WithIncorrectAmountValue_ReturnsBadRequest(double? amountValue)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = (decimal?)amountValue
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData((OperationType)99)]
    public async Task Update_RegularTransaction_WithIncorrectType_ReturnsBadRequest(OperationType? type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Type = type
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithIncorrectCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            CategoryId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithEmptyCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            CategoryId = Guid.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithIncorrectWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            WalletId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithEmptyWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            WalletId = Guid.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithEmptyId_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{Guid.Empty}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithIncorrectId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{Guid.NewGuid()}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    // UPDATE TRANSFER TRANSACTION tests
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewAmount_UpdatesTransactionAmountOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = 400M
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(upsert.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.SourceWalletId.Should().Be(scenario.transaction.SourceWalletId);
        updatedTransaction.TargetWalletId.Should().Be(scenario.transaction.TargetWalletId);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewDescription_UpdatesTransactionDescriptionOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Description = $"Updated Description + {Guid.NewGuid()}",
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(upsert.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.SourceWalletId.Should().Be(scenario.transaction.SourceWalletId);
        updatedTransaction.TargetWalletId.Should().Be(scenario.transaction.TargetWalletId);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewTransactionDate_UpdatesTransactionDateOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            TransactionDate = DateTimeOffset.UtcNow
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(upsert.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.SourceWalletId.Should().Be(scenario.transaction.SourceWalletId);
        updatedTransaction.TargetWalletId.Should().Be(scenario.transaction.TargetWalletId);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewSourceWallet_UpdatesTransactionSourceWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId = scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Wallets.Add(newWallet);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            SourceWalletId = newWallet.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.SourceWalletId.Should().Be(upsert.SourceWalletId.Value);
        updatedTransaction.TargetWalletId.Should().Be(scenario.transaction.TargetWalletId);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewTargetWallet_UpdatesTransactionTargetWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId = scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Wallets.Add(newWallet);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            TargetWalletId = newWallet.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.SourceWalletId.Should().Be(scenario.transaction.SourceWalletId);
        updatedTransaction.TargetWalletId.Should().Be(upsert.TargetWalletId.Value);
    }

    [Fact]
    public async Task Update_TransferTransaction_WithNewAmount_ShouldUpdateSourceAndTargetWalletBalances()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        var expectedSourceWalletBalance = scenario.wallets[0].Balance + scenario.transaction.Amount - upsert.Amount;
        var expectedTargetWalletBalance = scenario.wallets[1].Balance - scenario.transaction.Amount + upsert.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.SourceWalletId);
        var updatedTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.TargetWalletId);
        updatedSourceWallet.Should().NotBeNull();
        updatedTargetWallet.Should().NotBeNull();
        updatedSourceWallet.Balance.Should().Be(expectedSourceWalletBalance);
        updatedTargetWallet.Balance.Should().Be(expectedTargetWalletBalance);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewSourceWallet_ShouldUpdateOldAndNewWalletsBalances()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newSourceWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId =  scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Wallets.Add(newSourceWallet);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            SourceWalletId = newSourceWallet.Id
        };
        
        var expectedOldSourceWalletBalance = scenario.wallets[0].Balance + scenario.transaction.Amount;
        var expectedNewSourceWalletBalance = newSourceWallet.Balance - scenario.transaction.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedOldSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.SourceWalletId);
        var updatedNewSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == newSourceWallet.Id);
        updatedOldSourceWallet.Should().NotBeNull();
        updatedNewSourceWallet.Should().NotBeNull();
        updatedOldSourceWallet.Balance.Should().Be(expectedOldSourceWalletBalance);
        updatedNewSourceWallet.Balance.Should().Be(expectedNewSourceWalletBalance);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewTargetWallet_ShouldUpdateOldAndNewWalletsBalances()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newTargetWallet = DataFactory.CreateWallet(w =>
        {
            w.BudgetId =  scenario.budget.Id;
            w.CurrencyId = scenario.currency.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Wallets.Add(newTargetWallet);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            TargetWalletId = newTargetWallet.Id
        };
        
        var expectedOldTargetWalletBalance = scenario.wallets[1].Balance - scenario.transaction.Amount;
        var expectedNewTargetWalletBalance = newTargetWallet.Balance + scenario.transaction.Amount;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedOldTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.TargetWalletId);
        var updatedNewTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == newTargetWallet.Id);
        updatedOldTargetWallet.Should().NotBeNull();
        updatedNewTargetWallet.Should().NotBeNull();
        updatedOldTargetWallet.Balance.Should().Be(expectedOldTargetWalletBalance);
        updatedNewTargetWallet.Balance.Should().Be(expectedNewTargetWalletBalance);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", (TransferTransactionUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(-1.0)]
    public async Task Update_TransferTransaction_WithIncorrectAmountValue_ReturnsBadRequest(double amountValue)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = (decimal)amountValue
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewSourceWalletOfDifferentBudget_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newBudget = DataFactory.CreateBudget(b => b.CurrencyId = scenario.currency.Id);
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.CurrencyId = scenario.currency.Id;
            w.BudgetId = newBudget.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budget, newBudget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Wallets.Add(newWallet);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            SourceWalletId = newWallet.Id
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewTargetWalletOfDifferentBudget_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newBudget = DataFactory.CreateBudget(b => b.CurrencyId = scenario.currency.Id);
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.CurrencyId = scenario.currency.Id;
            w.BudgetId = newBudget.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budget, newBudget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Wallets.Add(newWallet);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            TargetWalletId = newWallet.Id
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithIncorrectSourceWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            SourceWalletId = Guid.NewGuid()
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithEmptySourceWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            SourceWalletId = Guid.Empty
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithIncorrectTargetWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            TargetWalletId = Guid.NewGuid()
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithEmptyTargetWalletId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            TargetWalletId = Guid.Empty
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithEmptyId_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{Guid.Empty}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithIncorrectId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{Guid.NewGuid()}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // DELETE tests
    
    [Fact]
    public async Task HardDelete_RegularTransaction_WithCorrectData_DeletesTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deleted = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task HardDelete_RegularTransaction_WalletIsNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.wallet.Id);
        existingWallet.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_RegularTransaction_CategoryIsNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCategory = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.category.Id);
        existingCategory.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_RegularTransaction_IncomeType_WalletBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Income,
            configureTransaction: t => t.Type = OperationType.Income
            );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedWalletBalance = scenario.wallet.Balance - scenario.transaction.Amount;

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedWalletBalance);
    }
    
    [Fact]
    public async Task HardDelete_RegularTransaction_ExpenseType_WalletBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Expense,
            configureTransaction: t => t.Type = OperationType.Expense
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedWalletBalance = scenario.wallet.Balance + scenario.transaction.Amount;

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedWalletBalance);
    }
    
    [Fact]
    public async Task HardDelete_RegularTransaction_IncomeType_WalletIsPartOfGeneralBalance_BudgetBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Income,
            configureTransaction: t => t.Type = OperationType.Income,
            configureWallet: w => w.IsPartOfGeneralBalance = true
            );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedBudgetBalance = scenario.budget.OverallBalance - scenario.transaction.Amount;

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Fact]
    public async Task HardDelete_RegularTransaction_ExpenseType_WalletIsPartOfGeneralBalance_BudgetBalanceIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = OperationType.Expense,
            configureTransaction: t => t.Type = OperationType.Expense,
            configureWallet: w => w.IsPartOfGeneralBalance = true
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedBudgetBalance = scenario.budget.OverallBalance + scenario.transaction.Amount;

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task HardDelete_RegularTransaction_WalletIsNotPartOfGeneralBalance_BudgetBalanceIsNotUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type,
            configureWallet: w => w.IsPartOfGeneralBalance = false
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.OverallBalance.Should().Be(scenario.budget.OverallBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task HardDelete_RegularTransaction_TransactionMeetsGoalConditions_GoalActualMoneyAmountIsUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t =>
            {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = goal.ActualMoneyAmount - scenario.transaction.Amount;
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task HardDelete_RegularTransaction_TransactionDoesntMeetGoalConditions_GoalActualMoneyAmountIsNotUpdated(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t =>
            {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(1);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = goal.ActualMoneyAmount;
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Fact]
    public async Task HardDelete_TransferTransaction_WithCorrectData_DeletesTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
        deletedTransaction.Should().BeNull();
    }

    [Fact]
    public async Task HardDelete_TransferTransaction_SourceAndTargetWalletsBalancesAreUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedSourceWalletBalance = scenario.wallets[0].Balance + scenario.transaction.Amount;
        var expectedTargetWalletBalance = scenario.wallets[1].Balance - scenario.transaction.Amount;
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.SourceWalletId);
        var updatedTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.TargetWalletId);
        updatedSourceWallet.Should().NotBeNull();
        updatedTargetWallet.Should().NotBeNull();
        updatedSourceWallet.Balance.Should().Be(expectedSourceWalletBalance);
        updatedTargetWallet.Balance.Should().Be(expectedTargetWalletBalance);
    }
    
    [Fact]
    public async Task HardDelete_TransferTransaction_SourceWalletIsNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.SourceWalletId);
        existingSourceWallet.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_TransferTransaction_TargetWalletIsNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.TargetWalletId);
        existingTargetWallet.Should().NotBeNull();
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HardDelete_TransferTransaction_BudgetBalanceIsNotUpdated(bool isPartOfGeneralBalance)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario(
            configureWallet: w => w.IsPartOfGeneralBalance = isPartOfGeneralBalance
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var budget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        budget.Should().NotBeNull();
        budget.OverallBalance.Should().Be(scenario.budget.OverallBalance);
    }
    
    [Fact]
    public async Task HardDelete_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task HardDelete_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}