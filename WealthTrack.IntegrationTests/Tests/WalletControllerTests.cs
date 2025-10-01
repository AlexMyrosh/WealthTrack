using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Data.DomainModels;
using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("WalletTests")]
public class WalletControllerTests(SeededWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET ALL TESTS

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_ShouldReturnCorrectNumberOfWallets(int numberOfWallets)
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets(numberOfWallets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/wallet");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().NotBeNullOrEmpty();
        walletsFromResponse.Should().HaveCount(numberOfWallets);
    }
    
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ShouldReturnWalletsWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/wallet");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }

    [Fact]
    public async Task GetAll_WithIncludedCurrency_ShouldReturnWalletsWithLoadingCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Currency)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().NotBeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedBudget_ShouldReturnWalletsWithLoadingBudgetOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Budget)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().NotBeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedTransactions_ShouldReturnWalletsWithLoadingTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Transactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().NotBeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedIncomeTransferTransactions_ShouldReturnWalletsWithLoadingIncomeAndOutgoingTransferTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.IncomeTransferTransactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().NotBeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedOutgoingTransferTransactions_ShouldReturnWalletsWithLoadingIncomeAndOutgoingTransferTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.OutgoingTransferTransactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().NotBeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludeAllRelatedEntities_ShouldReturnWalletsWithLoadingAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include=" +
                                             $"{nameof(Wallet.Budget)}," +
                                             $"{nameof(Wallet.Currency)}," +
                                             $"{nameof(Wallet.Transactions)}," +
                                             $"{nameof(Wallet.IncomeTransferTransactions)}," +
                                             $"{nameof(Wallet.OutgoingTransferTransactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().NotBeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Budget.Should().NotBeNull());
        walletsFromResponse.Should().AllSatisfy(w => w.Transactions.Should().NotBeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().NotBeNullOrEmpty());
        walletsFromResponse.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/wallet?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
    
    #region GET BY ID TESTS
    
    [Fact]
    public async Task GetById_ShouldReturnWalletWithCorrectId()
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Id.Should().Be(walletId);
    }

    [Fact]
    public async Task GetById_WithoutIncludeParameter_ShouldReturnWalletWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
        walletFromResponse.Budget.Should().BeNull();
        walletFromResponse.Transactions.Should().BeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().BeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedCurrency_ShouldReturnWalletWithCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.Currency)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().NotBeNull();
        walletFromResponse.Budget.Should().BeNull();
        walletFromResponse.Transactions.Should().BeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().BeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedBudget_ShouldReturnWalletWithBudgetOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.Budget)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
        walletFromResponse.Budget.Should().NotBeNull();
        walletFromResponse.Transactions.Should().BeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().BeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedTransactions_ShouldReturnWalletWithTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.Transactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
        walletFromResponse.Budget.Should().BeNull();
        walletFromResponse.Transactions.Should().NotBeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().BeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedIncomeTransferTransactions_ShouldReturnWalletWithIncomeTransferTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.IncomeTransferTransactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
        walletFromResponse.Budget.Should().BeNull();
        walletFromResponse.Transactions.Should().BeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedOutgoingTransferTransactions_ShouldReturnWalletWithOutgoingTransferTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.OutgoingTransferTransactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
        walletFromResponse.Budget.Should().BeNull();
        walletFromResponse.Transactions.Should().BeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().BeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedAllRelatedEntities_ShouldReturnWalletWithAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include=" +
                                             $"{nameof(Wallet.Budget)}," +
                                             $"{nameof(Wallet.Currency)}," +
                                             $"{nameof(Wallet.Transactions)}," +
                                             $"{nameof(Wallet.IncomeTransferTransactions)}," +
                                             $"{nameof(Wallet.OutgoingTransferTransactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().NotBeNull();
        walletFromResponse.Budget.Should().NotBeNull();
        walletFromResponse.Transactions.Should().NotBeNullOrEmpty();
        walletFromResponse.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        walletFromResponse.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{Guid.NewGuid()}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
    
    #region CREATE TESTS

    [Fact] public async Task Create_WithCorrectData_ShouldCreateNewWalletWithCorrectDefaultData()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var created = await DbContext.Wallets.AsNoTracking()
            .Include(w => w.Currency)
            .Include(w => w.Budget)
            .Include(w => w.Transactions)
            .Include(w => w.IncomeTransferTransactions)
            .Include(w => w.OutgoingTransferTransactions)
            .FirstOrDefaultAsync(w => w.Id == createdId);
        created.Should().NotBeNull();
        created.Name.Should().Be(upsert.Name);
        created.Balance.Should().Be(upsert.Balance);
        created.IsPartOfGeneralBalance.Should().Be(upsert.IsPartOfGeneralBalance.Value);
        created.Type.Should().Be(upsert.Type);
        created.Status.Should().Be(WalletStatus.Active);
        created.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        created.ModifiedDate.Should().BeExactly(created.CreatedDate);
        created.Currency.Id.Should().Be(upsert.CurrencyId.Value);
        created.Budget.Id.Should().Be(upsert.BudgetId.Value);
        created.IncomeTransferTransactions.Should().BeNullOrEmpty();
        created.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task Create_WhenAsPartOfGeneralBalanceIsTrue_ShouldUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = true,
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(upsert.Balance);
    }
    
    [Fact]
    public async Task Create_WhenAsPartOfGeneralBalanceIsFalse_ShouldNotUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = false,
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(0M);
    }
    
    [Theory]
    [InlineData(1, OperationType.Income)]
    [InlineData(-1, OperationType.Expense)]
    public async Task Create_CreatesWalletWithInitialBalance_ShouldCreateCorrectionTransaction(int initialBalanceSign, OperationType operationType)
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000) * initialBalanceSign,
            IsPartOfGeneralBalance = false,
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
        var expectedTransactionAmount =  decimal.Abs(upsert.Balance.Value);
        var balanceCorrectionCategoryId = factory.Configuration["SystemCategories:BalanceCorrectionId"];
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var allTransactions = await DbContext.Transactions.AsNoTracking().ToListAsync();
        allTransactions.Should().HaveCount(1);
        allTransactions[0].Amount.Should().Be(expectedTransactionAmount);
        allTransactions[0].Type.Should().Be(operationType);
        allTransactions[0].CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
    
    [Fact]
    public async Task Create_WithNullBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", (WalletUpsertApiModel)null!);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = string.Empty,
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullName_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = null,
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullBalance_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = null,
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullIsPartOfGeneralBalance_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = null,
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData((WalletType)99)]
    public async Task Create_WithIncorrectType_ShouldReturnBadRequest(WalletType? type)
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = type,
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = null,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = Guid.NewGuid(),
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = Guid.Empty,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullBudgetId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = null
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectBudgetId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = Guid.NewGuid()
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyBudgetId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = Guid.Empty
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UPDATE TESTS

    [Fact]
    public async Task Update_WithNewName_ShouldUpdateNameOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Updated Name + {Guid.NewGuid()}"
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallets[0].Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking()
            .Include(wallet => wallet.Transactions)
            .Include(wallet => wallet.IncomeTransferTransactions)
            .Include(wallet => wallet.OutgoingTransferTransactions)
            .FirstOrDefaultAsync(w => w.Id == scenario.wallets[0].Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Name.Should().Be(upsert.Name);
        updatedWallet.Balance.Should().Be(scenario.wallets[0].Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(scenario.wallets[0].IsPartOfGeneralBalance);
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.CreatedDate.Should().BeExactly(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewBalance_ShouldUpdateBalanceOnly()
    {
      // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Balance = Random.Next(100, 1000)
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallets[0].Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking()
            .Include(wallet => wallet.Transactions)
            .Include(wallet => wallet.IncomeTransferTransactions)
            .Include(wallet => wallet.OutgoingTransferTransactions)
            .FirstOrDefaultAsync(w => w.Id == scenario.wallets[0].Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Name.Should().Be(scenario.wallets[0].Name);
        updatedWallet.Balance.Should().Be(upsert.Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(scenario.wallets[0].IsPartOfGeneralBalance);
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.CreatedDate.Should().BeExactly(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewIsPartOfGeneralBalance_ShouldUpdateIsPartOfGeneralBalanceOnly()
    {
      // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            IsPartOfGeneralBalance = !scenario.wallets[0].IsPartOfGeneralBalance
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallets[0].Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking()
            .Include(wallet => wallet.Transactions)
            .Include(wallet => wallet.IncomeTransferTransactions)
            .Include(wallet => wallet.OutgoingTransferTransactions)
            .FirstOrDefaultAsync(w => w.Id == scenario.wallets[0].Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Name.Should().Be(scenario.wallets[0].Name);
        updatedWallet.Balance.Should().Be(scenario.wallets[0].Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(upsert.IsPartOfGeneralBalance.Value);
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.CreatedDate.Should().BeExactly(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewCurrency_ShouldUpdateCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        var newCurrency = DataFactory.CreateCurrency();
        DbContext.Currencies.AddRange(scenario.currency, newCurrency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            CurrencyId = newCurrency.Id
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallets[0].Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedWallet = await DbContext.Wallets.AsNoTracking()
            .Include(wallet => wallet.Transactions)
            .Include(wallet => wallet.IncomeTransferTransactions)
            .Include(wallet => wallet.OutgoingTransferTransactions)
            .FirstOrDefaultAsync(w => w.Id == scenario.wallets[0].Id);
        updatedWallet.Should().NotBeNull();
        updatedWallet.Name.Should().Be(scenario.wallets[0].Name);
        updatedWallet.Balance.Should().Be(scenario.wallets[0].Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(scenario.wallets[0].IsPartOfGeneralBalance);
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.CreatedDate.Should().BeExactly(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(upsert.CurrencyId.Value);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Theory]
    [InlineData(false, true, 1)]
    [InlineData(true, false, -1)]
    public async Task Update_WithNewIsPartOfGeneralBalance_ShouldUpdateBudgetBalance(bool initialValue, bool valueToUpdate, int operationSign)
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions(configureWallets: w => w.IsPartOfGeneralBalance = initialValue);
        DbContext.Currencies.AddRange(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var expectedOverallBalance = scenario.budget.OverallBalance + scenario.wallets[0].Balance * operationSign;
        
        var upsert = new WalletUpsertApiModel
        {
            IsPartOfGeneralBalance = valueToUpdate
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallets[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.OverallBalance.Should().Be(expectedOverallBalance);
    }
    
    [Fact]
    public async Task Update_UpdateWalletBalanceWithBiggerAmount_ShouldCreateIncomeCorrectionTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        var balanceCorrectionCategoryId = factory.Configuration["SystemCategories:BalanceCorrectionId"];
        var upsert = new WalletUpsertApiModel
        {
            Balance = scenario.wallet.Balance + Random.Next(100, 1000)
        };
        
        var expectedTransactionAmount = upsert.Balance - scenario.wallet.Balance;
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var correctionTransaction = await DbContext.Transactions.AsNoTracking().SingleOrDefaultAsync();
        correctionTransaction.Should().NotBeNull();
        correctionTransaction.Amount.Should().Be(expectedTransactionAmount);
        correctionTransaction.Type.Should().Be(OperationType.Income);
        correctionTransaction.CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
    
    [Fact]
    public async Task Update_UpdateWalletBalanceWithSmallerAmount_ShouldCreateExpenseCorrectionTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        var balanceCorrectionCategoryId = factory.Configuration["SystemCategories:BalanceCorrectionId"];
        var upsert = new WalletUpsertApiModel
        {
            Balance = scenario.wallet.Balance - Random.Next(100, 1000)
        };
        
        var expectedTransactionAmount =  scenario.wallet.Balance - upsert.Balance;
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var correctionTransaction = await DbContext.Transactions.AsNoTracking().SingleOrDefaultAsync();
        correctionTransaction.Should().NotBeNull();
        correctionTransaction.Amount.Should().Be(expectedTransactionAmount);
        correctionTransaction.Type.Should().Be(OperationType.Expense);
        correctionTransaction.CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
    
    [Fact]
    public async Task Update_WhenWalletBalanceIsNotChanged_ShouldNotCreateAnyTransactions()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        var upsert = new WalletUpsertApiModel
        {
            Balance = scenario.wallet.Balance
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var correctionTransaction = await DbContext.Transactions.AsNoTracking().ToListAsync();
        correctionTransaction.Should().BeNullOrEmpty();
    }
        
    [Fact]
    public async Task Update_UpdateWalletBalance_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(configureWallet: w => w.IsPartOfGeneralBalance = true);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        var upsert = new WalletUpsertApiModel
        {
            Balance = Random.Next(100, 1000)
        };
        var expectedBalance = scenario.budget.OverallBalance - scenario.wallet.Balance + upsert.Balance;
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(expectedBalance);
    }
    
    [Fact]
    public async Task Update_UpdateWalletBalance_WhenWalletIsNotPartOfGeneralBalance_ShouldNotUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(configureWallet: w => w.IsPartOfGeneralBalance = false);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        var upsert = new WalletUpsertApiModel
        {
            Balance = Random.Next(100, 1000)
        };
        var expectedBalance = scenario.budget.OverallBalance;
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(expectedBalance);
    }
    
    [Fact]
    public async Task Update_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            Name = $"Updated Wallet Name + {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{Guid.Empty}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            Name = $"Updated Wallet Name + {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{Guid.NewGuid()}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Update_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", (WalletUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            Name = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(WalletType.Cash, WalletType.DebitCard)]
    [InlineData(WalletType.DebitCard, WalletType.Cash)]
    public async Task Update_WithNewType_ShouldReturnBadRequest(WalletType oldType, WalletType newType)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(configureWallet: w => w.Type = oldType);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            Type = newType
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            CurrencyId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            CurrencyId = Guid.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithNewBudget_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var newBudget = DataFactory.CreateBudget(b => b.CurrencyId = scenario.currency.Id);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budget, newBudget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            BudgetId = newBudget.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion
    
    #region DELETE TESTS

    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldDeleteWallet()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        deletedWallet.Should().BeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldDeleteRelatedTransactions()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallets[0].Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedTransactions = await DbContext.Transactions.AsNoTracking().Where(t => t.WalletId == scenario.wallets[0].Id).ToListAsync();
        var deletedTransferTransactions = await DbContext.TransferTransactions.AsNoTracking().Where(t =>
            t.SourceWalletId == scenario.wallets[0].Id || t.TargetWalletId == scenario.wallets[0].Id).ToListAsync();
        deletedTransactions.Should().BeNullOrEmpty();
        deletedTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteRelatedBudget()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        existingBudget.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteRelatedCurrency()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.currency.Id);
        existingCurrency.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteRelatedTransactionCategory()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.AddRange(scenario.transaction);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCategory = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.category.Id);
        existingCategory.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WhenWalletWasAsPartOfGeneralBalance_ShouldUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(configureWallet: w => w.IsPartOfGeneralBalance = true);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        var expectedBudgetBalance = scenario.budget.OverallBalance - scenario.wallet.Balance;
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Fact]
    public async Task HardDelete_WhenWalletWasNotAsPartOfGeneralBalance_ShouldNotUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(configureWallet: w => w.IsPartOfGeneralBalance = false);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        var expectedBudgetBalance = scenario.budget.OverallBalance;
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(expectedBudgetBalance);
    }
    
    [Fact]
    public async Task HardDelete_WhenTransferTransactionExists_ShouldUpdateTargetWalletBalance()
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
        var expectedTargetWalletBalance = targetWallet.Balance - scenario.transaction.Amount;
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{sourceWallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTargetWallet = await DbContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == targetWallet.Id);
        updatedTargetWallet.Should().NotBeNull();
        updatedTargetWallet.Balance.Should().Be(expectedTargetWalletBalance);
    }
    
    [Fact]
    public async Task HardDelete_WhenTransferTransactionExists_ShouldUpdateSourceWalletBalance()
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
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{targetWallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedSourceWallet = await DbContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == sourceWallet.Id);
        updatedSourceWallet.Should().NotBeNull();
        updatedSourceWallet.Balance.Should().Be(expectedSourceWalletBalance);
    }
    
    [Fact]
    public async Task HardDelete_WithIncorrectId_ShouldReturnNotFoundResult() 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{Guid.NewGuid()}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task HardDelete_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}