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
public class TransactionControllerTests(SeededWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET ALL TESTS

    [Theory]
    [InlineData(2,2)]
    [InlineData(4,4)]
    [InlineData(6,6)]
    public async Task GetAll_ShouldReturnCorrectNumberOfTransactions(int numberOfTransactions, int numberOfTransfers)
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
        var expectedNumberOfTransactions = numberOfTransactions + numberOfTransfers;

        // Act
        var response = await Client.GetAsync("/api/transaction");
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().NotBeNullOrEmpty();
        transactionsFromResponse.Count.Should().Be(expectedNumberOfTransactions);
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnTransactionsOrderedByTransactionDate()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/transaction");
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().BeInDescendingOrder(t => t.TransactionDate);
    }
    
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ShouldReturnTransactionsWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/transaction");
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        transactionsFromResponse.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        transactionsFromResponse.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        transactionsFromResponse.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedCategory_RegularTransactions_ShouldReturnTransactionsWithCategoryOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
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
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        transactionsFromResponse.Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
        transactionsFromResponse.Where(t => regularTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().NotBeNull());
        transactionsFromResponse.Where(t => !regularTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedWallet_RegularTransactions_ShouldReturnTransactionsWithWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
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
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().AllSatisfy(t => t.Category.Should().BeNull());
        transactionsFromResponse.Where(t => regularTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Wallet.Should().NotBeNull());
        transactionsFromResponse.Where(t => !regularTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedSourceWallet_TransferTransactions_ShouldReturnTransactionsWithSourceWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
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
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().NotBeNullOrEmpty();
        transactionsFromResponse.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.SourceWallet.Should().NotBeNull());
        transactionsFromResponse.Where(t => !transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        transactionsFromResponse.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedTargetWallet_TransferTransactions_ShouldReturnTransactionsWithTargetWalletOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
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
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().NotBeNullOrEmpty();
        transactionsFromResponse.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.TargetWallet.Should().NotBeNull());
        transactionsFromResponse.Should().AllSatisfy(t => t.Category.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncludeAllRelatedEntities_ShouldReturnTransactionsWithAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
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
        var transactionsFromResponse = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionsFromResponse.Should().NotBeNullOrEmpty();
        transactionsFromResponse.Where(t => transactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Wallet.Should().NotBeNull());
        transactionsFromResponse.Where(t => !transactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Wallet.Should().BeNull());
        transactionsFromResponse.Where(t => transactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().NotBeNull());
        transactionsFromResponse.Where(t => !transactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.Category.Should().BeNull());
        transactionsFromResponse.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.SourceWallet.Should().NotBeNull());
        transactionsFromResponse.Where(t => !transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.SourceWallet.Should().BeNull());
        transactionsFromResponse.Where(t => transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.TargetWallet.Should().NotBeNull());
        transactionsFromResponse.Where(t => !transferTransactionIds.Contains(t.Id)).Should().AllSatisfy(t => t.TargetWallet.Should().BeNull());
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/transaction?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
    
    #region GET BY ID TESTS
    
    [Fact]
    public async Task GetById_RegularTransaction_ShouldReturnTransactionWithCorrectId()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Id.Should().Be(scenario.transaction.Id);
    }
    
    [Fact]
    public async Task GetById_TransferTransactions_ShouldReturnTransactionWithCorrectId()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}");
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Id.Should().Be(scenario.transaction.Id);
    }
    
    [Fact]
    public async Task GetById_WithoutIncludeParameter_RegularTransaction_ShouldReturnTransactionWithoutLoadingRelatedEntities()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().BeNull();
        transactionFromResponse.Category.Should().BeNull();
        transactionFromResponse.SourceWallet.Should().BeNull();
        transactionFromResponse.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncludedCategory_RegularTransaction_ShouldReturnTransactionWithCategoryOnly()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().BeNull();
        transactionFromResponse.Category.Should().NotBeNull();
        transactionFromResponse.SourceWallet.Should().BeNull();
        transactionFromResponse.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncludedWallet_RegularTransaction_ShouldReturnTransactionWithWalletOnly()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().NotBeNull();
        transactionFromResponse.Category.Should().BeNull();
        transactionFromResponse.SourceWallet.Should().BeNull();
        transactionFromResponse.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncludedCategoryAndWallet_RegularTransaction_ShouldReturnTransactionWithCategoryAndWalletOnly()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().NotBeNull();
        transactionFromResponse.Category.Should().NotBeNull();
        transactionFromResponse.SourceWallet.Should().BeNull();
        transactionFromResponse.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncludedSourceWallet_TransferTransactions_ShouldReturnTransferWithSourceWalletOnly()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().BeNull();
        transactionFromResponse.Category.Should().BeNull();
        transactionFromResponse.SourceWallet.Should().NotBeNull();
        transactionFromResponse.TargetWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncludedTargetWallet_TransferTransactions_ShouldReturnTransferWithTargetWalletOnly()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().BeNull();
        transactionFromResponse.Category.Should().BeNull();
        transactionFromResponse.SourceWallet.Should().BeNull();
        transactionFromResponse.TargetWallet.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncludedSourceAndTargetWallets_TransferTransactions_ShouldReturnTransferWithSourceAndTargetWalletsOnly()
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
        var transactionFromResponse = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        transactionFromResponse.Should().NotBeNull();
        transactionFromResponse.Wallet.Should().BeNull();
        transactionFromResponse.Category.Should().BeNull();
        transactionFromResponse.SourceWallet.Should().NotBeNull();
        transactionFromResponse.TargetWallet.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_RegularTransaction_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/transaction/{scenario.transaction.Id}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_TransferTransactions_ShouldReturnBadRequest()
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

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
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
    
    [Fact]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
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

    #endregion
    
    #region CREATE REGULAR TRANSACTION TESTS

    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Create_RegularTransaction_WithCorrectData_ShouldCreateNewTransactionWithCorrectDefaultData(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(c => c.Type = type);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
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
        created.ModifiedDate.Should().BeExactly(created.CreatedDate);
        created.Type.Should().Be(upsert.Type);
        created.CategoryId.Should().Be(upsert.CategoryId.Value);
        created.WalletId.Should().Be(upsert.WalletId.Value);
    }

    [Theory]
    [InlineData(OperationType.Expense, -1)]
    [InlineData(OperationType.Income, 1)]
    public async Task Create_RegularTransaction_ShouldUpdateWalletBalance(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(c => c.Type = type);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.wallet.Balance + upsert.Amount * operationSign;

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedBalanceAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Expense, -1)]
    [InlineData(OperationType.Income, 1)]
    public async Task Create_RegularTransaction_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = type,
            configureWallet: w => w.IsPartOfGeneralBalance = true
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
            WalletId = scenario.wallet.Id
        };

        var expectedBalanceAmount = scenario.budget.OverallBalance + upsert.Amount * operationSign;

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
    public async Task Create_RegularTransaction_WhenWalletIsNotPartOfGeneralBalance_ShouldNotUpdateBudgetBalance(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = type,
            configureWallet: w => w.IsPartOfGeneralBalance = false
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
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
    public async Task Create_RegularTransaction_WhenAddedTransactionIsApplicableForGoal_ShouldUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(c => c.Type = type);
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
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
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
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_RegularTransaction_WhenAddedTransactionIsNotApplicableForGoal_WhenTypeAndCategoryAreDifferent_ShouldNotUpdateGoalActualMoneyAmount(OperationType transactionType, OperationType goalType)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = transactionType
        );
        var goalCategory = DataFactory.CreateCategory(c => c.Type = goalType);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = goalType;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [goalCategory];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category, goalCategory);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = transactionType,
            CategoryId = scenario.category.Id,
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
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_RegularTransaction_WhenAddedTransactionIsNotApplicableForGoal_WhenCategoryIsDifferent_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = type
        );
        var goalCategory = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [goalCategory];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category, goalCategory);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
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
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_RegularTransaction_WhenAddedTransactionIsNotApplicableForGoal_WhenTransactionDateIsBigger_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = type
        );
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddMinutes(-1);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
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
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_RegularTransaction_WhenAddedTransactionIsNotApplicableForGoal_WhenTransactionDateIsLess_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = type
        );
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(1);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [scenario.category];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
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
    
    [Fact]
    public async Task Create_RegularTransaction_WithNullBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", (TransactionUpsertApiModel)null!);
        
        // Arrange
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(-1.0)]
    [InlineData(null)]
    public async Task Create_RegularTransaction_WithIncorrectAmountValue_ShouldReturnBadRequest(double? amountValue)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = (decimal?)amountValue!,
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithNullTransactionDate_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = null,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
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
    public async Task Create_RegularTransaction_WithIncorrectType_ShouldReturnBadRequest(OperationType? type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = type,
            CategoryId = scenario.category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_RegularTransaction_WithCategoryOfAnotherTypeThanTransaction_ShouldReturnBadRequest(OperationType transactionType, OperationType categoryType)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory(
            configureCategory: c => c.Type = categoryType
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = transactionType,
            CategoryId = scenario.category.Id,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithIncorrectCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = Guid.NewGuid(),
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithEmptyCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = Guid.Empty,
            WalletId = scenario.wallet.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithIncorrectWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
            WalletId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithEmptyWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
            WalletId = Guid.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_RegularTransaction_WithNullWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithCategory();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transaction + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = scenario.category.Type,
            CategoryId = scenario.category.Id,
            WalletId = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region CREATE TRANSFER TRANSACTION TESTS

    [Fact]
    public async Task Create_TransferTransaction_WithCorrectData_ShouldCreateNewTransactionWithCorrectDefaultData()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1, scenario.wallet2);
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
        createdTransfer.ModifiedDate.Should().BeExactly(createdTransfer.CreatedDate);
        createdTransfer.SourceWalletId.Should().Be(upsert.SourceWalletId.Value);
        createdTransfer.TargetWalletId.Should().Be(upsert.TargetWalletId.Value);
    }

    [Fact]
    public async Task Create_TransferTransaction_ShouldUpdateSourceAndTargetWalletBalances()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet1, scenario.wallet2);
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
    public async Task Create_TransferTransaction_SourceAndTargetWalletFromDifferentBudgets_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletsForDifferentBudgets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budget1, scenario.budget2);
        DbContext.Wallets.AddRange(scenario.wallet1, scenario.wallet2);
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

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_TransferTransaction_WithNullBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/create", (TransactionUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(-1.0)]
    [InlineData(null)]
    public async Task Create_TransferTransaction_WithIncorrectAmountValue_ShouldReturnBadRequest(double? amountValue)
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithNullTransactionDate_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithNullSourceWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithIncorrectSourceWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithEmptySourceWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithNullTargetWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithIncorrectTargetWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    public async Task Create_TransferTransaction_WithEmptyTargetWalletId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWallets();
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
    
    [Fact]
    public async Task Create_TransferTransaction_SourceAndTargetWalletIsTheSame_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        var upsert = new TransferTransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000),
            Description = $"Test Transfer + {Guid.NewGuid()}",
            TransactionDate = DateTimeOffset.UtcNow,
            SourceWalletId = scenario.wallet.Id,
            TargetWalletId = scenario.wallet.Id,
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
    
    #region UPDATE REGULAR TRANSACTION TESTS

    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_RegularTransaction_WithNewAmount_ShouldUpdateAmountOnly(OperationType type) 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
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
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(upsert.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_RegularTransaction_WithNewDescription_ShouldUpdateDescriptionOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
        );
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
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_RegularTransaction_WithNewTransactionDate_ShouldUpdateTransactionDateOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
        );
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
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_RegularTransaction_WithNewCategory_ShouldUpdateCategoryOnly(OperationType type)
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
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(upsert.CategoryId.Value);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_RegularTransaction_WithNewWallet_ShouldUpdateWalletOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
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

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(upsert.WalletId.Value);
    }
    
    [Theory]
    [InlineData(OperationType.Expense, -1)]
    [InlineData(OperationType.Income, 1)]
    public async Task Update_RegularTransaction_WithNewAmount_ShouldUpdateWalletBalance(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
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
        var expectedWalletBalance = scenario.wallet.Balance + (upsert.Amount - scenario.transaction.Amount) * operationSign;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedWalletBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Expense, -1)]
    [InlineData(OperationType.Income, 1)]
    public async Task Update_RegularTransaction_WithNewAmount_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type,
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
        
        var expectedBudgetBalance = scenario.budget.OverallBalance + (upsert.Amount - scenario.transaction.Amount) * operationSign;

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
        var expectedBudgetBalance = scenario.budget.OverallBalance;
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
        updatedBudget.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Income, 1)]
    [InlineData(OperationType.Expense, -1)]
    public async Task Update_RegularTransaction_WithNewWallet_ShouldUpdateWalletsBalances(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
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

        var expectedOldWalletBalance = scenario.wallet.Balance - scenario.transaction.Amount * operationSign;
        var expectedNewWalletBalance = newWallet.Balance + scenario.transaction.Amount * operationSign;

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
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewAmount_WhenApplicableGoalExists_ShouldUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionWithApplicableGoal(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type,
            configureGoal: g => g.Type = type
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        var upsert = new TransactionUpsertApiModel
        {
            Amount = Random.Next(100, 1000)
        };

        var expectedActualMoneyAmount = scenario.goal.ActualMoneyAmount - scenario.transaction.Amount + upsert.Amount;
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewAmount_WhenTransactionDateIsBiggerThanGoalEndDate_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.Now;
            });
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(30);
            g.EndDate = DateTimeOffset.UtcNow.AddMinutes(-1);
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
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewAmount_WhenTransactionDateIsLessThanGoalStartDate_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.Now;
            });
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(1);
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
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewAmount_WhenTransactionCategoryIsDifferentFromGoal_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.Now;
            });
        var secondCategory = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [secondCategory];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category, secondCategory);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = goal.ActualMoneyAmount;
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
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewTransactionDate_ToMakeTransactionApplicableForGoal_ShouldUpdateGoalActualMoneyAmount(OperationType type)
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
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(-1);
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
    public async Task Update_RegularTransaction_WithNewCategory_ToMakeTransactionApplicableForGoal_ShouldUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t =>
            {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var secondCategory = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [secondCategory];
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
            CategoryId = secondCategory.Id
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
    
    [Fact]
    public async Task Update_RegularTransaction_WithEmptyId_ShouldReturnBadRequest()
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
    public async Task Update_RegularTransaction_WithIncorrectId_ShouldReturnNotFoundResult()
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
    
    [Fact]
    public async Task Update_RegularTransaction_WithNullBody_ShouldReturnBadRequest()
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
    
    [Fact]
    public async Task Update_RegularTransaction_WithNegativeAmountValue_ShouldReturnBadRequest()
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
            Amount = -1M
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(OperationType.Expense, OperationType.Income)]
    [InlineData(OperationType.Income, OperationType.Expense)]
    public async Task Update_RegularTransaction_WithNewType_ShouldReturnBadRequest(OperationType oldType, OperationType newType)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = oldType,
            configureTransaction: t => t.Type = oldType
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var upsert = new TransactionUpsertApiModel
        {
            Type = newType
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithIncorrectCategoryId_ShouldReturnBadRequest()
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
    public async Task Update_RegularTransaction_WithEmptyCategoryId_ShouldReturnBadRequest()
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
    
    [Theory]
    [InlineData(OperationType.Expense, OperationType.Income)]
    [InlineData(OperationType.Income, OperationType.Expense)]
    public async Task Update_RegularTransaction_WithCategoryOfAnotherTypeThanTransaction_ShouldReturnBadRequest(OperationType transactionType, OperationType newCategoryType)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = transactionType,
            configureTransaction: t => t.Type = transactionType
        );
        var newCategory = DataFactory.CreateCategory(c => c.Type = newCategoryType);
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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_RegularTransaction_WithIncorrectWalletId_ShouldReturnBadRequest()
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
    public async Task Update_RegularTransaction_WithEmptyWalletId_ShouldReturnBadRequest()
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
    public async Task Update_RegularTransaction_WithNewWalletIdOfAnotherBudget_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        var newBudget = DataFactory.CreateBudget(b => b.CurrencyId = scenario.currency.Id);
        var newWallet = DataFactory.CreateWallet(w =>
        {
            w.CurrencyId = scenario.currency.Id;
            w.BudgetId = newBudget.Id;
        });
        
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budget, newBudget);
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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion

    #region UPDATE TRANSFER TRANSACTION TESTS

    [Fact]
    public async Task Update_TransferTransaction_WithNewAmount_ShouldUpdateAmountOnly()
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
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
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
    public async Task Update_TransferTransaction_WithNewDescription_ShouldUpdateDescriptionOnly()
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
            Description = $"Updated transfer Description + {Guid.NewGuid()}",
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
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
    public async Task Update_TransferTransaction_WithNewTransactionDate_ShouldUpdateTransactionDateOnly()
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
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
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
    public async Task Update_TransferTransaction_WithNewSourceWallet_ShouldUpdateSourceWalletOnly()
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
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
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
    public async Task Update_TransferTransaction_WithNewTargetWallet_ShouldUpdateTargetWalletOnly()
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
        var updatedTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
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
    public async Task Update_TransferTransaction_WithEmptyId_ShouldReturnBadRequest()
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
    public async Task Update_TransferTransaction_WithIncorrectId_ShouldReturnNotFoundResult()
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
    
    [Fact]
    public async Task Update_TransferTransaction_WithNullBody_ShouldReturnBadRequest()
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
    
    [Fact]
    public async Task Update_TransferTransaction_WithNegativeAmountValue_ShouldReturnBadRequest()
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
            Amount = -1M
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewSourceWalletOfDifferentBudget_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newBudget = DataFactory.CreateBudget(b => b.CurrencyId = scenario.currency.Id);
        var newWallet = DataFactory.CreateWallet(w => {
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
    public async Task Update_TransferTransaction_WithNewTargetWalletOfDifferentBudget_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        var newBudget = DataFactory.CreateBudget(b => b.CurrencyId = scenario.currency.Id);
        var newWallet = DataFactory.CreateWallet(w => {
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
    public async Task Update_TransferTransaction_WithIncorrectSourceWalletId_ShouldReturnBadRequest()
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
    public async Task Update_TransferTransaction_WithEmptySourceWalletId_ShouldReturnBadRequest()
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
    public async Task Update_TransferTransaction_WithIncorrectTargetWalletId_ShouldReturnBadRequest()
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
    public async Task Update_TransferTransaction_WithEmptyTargetWalletId_ShouldReturnBadRequest()
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
    public async Task Update_TransferTransaction_WithNewSourceWallet_SourceAndTargetWalletsAreTheSame_ShouldReturnBadRequest()
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
            SourceWalletId = scenario.transaction.TargetWalletId
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_TransferTransaction_WithNewTargetWallet_SourceAndTargetWalletsAreTheSame_ShouldReturnBadRequest()
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
            TargetWalletId = scenario.transaction.SourceWalletId
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{scenario.transaction.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion

    #region UNASSIGN CATEGORY TESTS

    [Fact]
    public async Task UnassignCategory_WithCorrectData_ShouldUnassignCategoryFromTransactionOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().BeNull();
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
        var category = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.category.Id);
        category.Should().NotBeNull();
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task UnassignCategory_WhenApplicableGoalExists_ShouldGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionWithApplicableGoal(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type,
            configureGoal: g => g.Type = type
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = scenario.goal.ActualMoneyAmount - scenario.transaction.Amount;
        
        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task UnassignCategory_WhenTransactionDateIsBiggerThanGoalEndDate_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.Now;
            });
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(30);
            g.EndDate = DateTimeOffset.UtcNow.AddMinutes(-1);
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
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task UnassignCategory_WhenTransactionDateIsLessThanGoalStartDate_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.Now;
            });
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(1);
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
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task UnassignCategory_WhenTransactionCategoryIsDifferentFromGoal_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.Now;
            });
        var secondCategory = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [secondCategory];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category, secondCategory);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = goal.ActualMoneyAmount;
        
        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Fact]
    public async Task UnassignCategory_WhenCategoryIsAlreadyEmpty_ShouldNotUpdateTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        scenario.transaction.CategoryId = null;
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction.Amount.Should().Be(scenario.transaction.Amount);
        updatedTransaction.Description.Should().Be(scenario.transaction.Description);
        updatedTransaction.TransactionDate.Should().BeExactly(scenario.transaction.TransactionDate);
        updatedTransaction.CreatedDate.Should().BeExactly(scenario.transaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().NotBe(updatedTransaction.CreatedDate);
        updatedTransaction.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedTransaction.Type.Should().Be(scenario.transaction.Type);
        updatedTransaction.CategoryId.Should().Be(scenario.transaction.CategoryId);
        updatedTransaction.WalletId.Should().Be(scenario.transaction.WalletId);
        var category = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.category.Id);
        category.Should().NotBeNull();
    }
    
    [Fact]
    public async Task UnassignCategory_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{Guid.Empty}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task UnassignCategory_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{Guid.NewGuid()}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UnassignCategory_WithTransferTransactionId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsync($"/api/transaction/unassign_category/{scenario.transaction.Id}", new StringContent(string.Empty));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE TESTS

    [Fact]
    public async Task HardDelete_RegularTransaction_WithCorrectData_ShouldDeleteTransaction()
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
        var deletedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == scenario.transaction.Id);
        deletedTransaction.Should().BeNull();
    }

    [Fact]
    public async Task HardDelete_RegularTransaction_ShouldNotDeleteWallet()
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
    public async Task HardDelete_RegularTransaction_ShouldNotDeleteCategory()
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
    
    [Theory]
    [InlineData(OperationType.Expense, -1)]
    [InlineData(OperationType.Income, 1)]
    public async Task HardDelete_RegularTransaction_ShouldUpdateWalletBalance(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedWalletBalance = scenario.wallet.Balance - scenario.transaction.Amount * operationSign;

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Balance.Should().Be(expectedWalletBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Expense, -1)]
    [InlineData(OperationType.Income, 1)]
    public async Task HardDelete_RegularTransaction_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance(OperationType type, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t => t.Type = type,
            configureWallet: w => w.IsPartOfGeneralBalance = true
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedBudgetBalance = scenario.budget.OverallBalance - scenario.transaction.Amount * operationSign;

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
    public async Task HardDelete_RegularTransaction_WalletIsNotPartOfGeneralBalance_ShouldNotUpdateBudgetBalance(OperationType type)
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
        var expectedBudgetBalance = scenario.budget.OverallBalance;

        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.budget.Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task HardDelete_RegularTransaction_WhenApplicableGoalExists_ShouldUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionWithApplicableGoal(configureCategory:  c => c.Type = type);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = scenario.goal.ActualMoneyAmount - scenario.transaction.Amount;
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task HardDelete_RegularTransaction_WhenTransactionDateIsBiggerThanGoalEndDate_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
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
            g.EndDate = DateTimeOffset.UtcNow.AddMinutes(-1);
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
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task HardDelete_RegularTransaction_WhenTransactionDateIsLessThanGoalStartDate_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
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
            g.StartDate =  DateTimeOffset.UtcNow.AddMinutes(1);
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
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task HardDelete_RegularTransaction_WhenTransactionCategoryIsDifferentFromGoal_ShouldNotUpdateGoalActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario(
            configureCategory: c => c.Type = type,
            configureTransaction: t =>
            {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var secondCategory = DataFactory.CreateCategory(c => c.Type = type);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = type;
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [secondCategory];
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.category, secondCategory);
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
    public async Task HardDelete_TransferTransaction_WithCorrectData_ShouldDeleteTransaction()
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
    public async Task HardDelete_TransferTransaction_ShouldNotDeleteSourceWallet()
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
    public async Task HardDelete_TransferTransaction_ShouldNotDeleteTargetWallet()
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

    [Fact]
    public async Task HardDelete_TransferTransaction_ShouldUpdateSourceAndTargetWalletsBalances()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var sourceWallet = scenario.wallets.First(w => w.Id == scenario.transaction.SourceWalletId);
        var targetWallet = scenario.wallets.First(w => w.Id == scenario.transaction.TargetWalletId);
        var expectedSourceWalletBalance = sourceWallet.Balance + scenario.transaction.Amount;
        var expectedTargetWalletBalance = targetWallet.Balance - scenario.transaction.Amount;
        
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
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HardDelete_TransferTransaction_ShouldNotUpdateBudgetBalance(bool isPartOfGeneralBalance)
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
        var expectedBudgetBalance = scenario.budget.OverallBalance;
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{scenario.transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var budget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        budget.Should().NotBeNull();
        budget.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Fact]
    public async Task HardDelete_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task HardDelete_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}