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
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletIds = scenario.wallets.Select(w => w.Id).ToList();

        // Act
        var response = await Client.GetAsync("/api/wallet");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().NotBeNullOrEmpty();
        walletsFromResponse.Should().HaveCount(numberOfWallets);
        walletsFromResponse.Should().AllSatisfy(w => walletIds.Should().Contain(w.Id));
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_ShouldReturnActiveWalletsOnly(int numberOfActiveWallets, int numberOfArchivedWallets)
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfActiveAndArchivedWallets(numberOfActiveWallets, numberOfArchivedWallets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.AddRange(scenario.activeWallets);
        DbContext.Wallets.AddRange(scenario.archivedWallets);
        await DbContext.SaveChangesAsync();
        var activeWalletIds = scenario.activeWallets.Select(b => b.Id).ToList();
        
        // Act
        var response = await Client.GetAsync("/api/wallet");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().NotBeNullOrEmpty();
        walletsFromResponse.Should().HaveCount(numberOfActiveWallets);
        walletsFromResponse.Should().AllSatisfy(b => activeWalletIds.Should().Contain(b.Id));
    }
    
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ShouldReturnWalletsWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/wallet");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Currency)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().NotBeNull());
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Transactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.IncomeTransferTransactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.OutgoingTransferTransactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().BeNull());
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include=" +
                                             $"{nameof(Wallet.Currency)}," +
                                             $"{nameof(Wallet.Transactions)}," +
                                             $"{nameof(Wallet.IncomeTransferTransactions)}," +
                                             $"{nameof(Wallet.OutgoingTransferTransactions)}");
        var walletsFromResponse = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletsFromResponse.Should().AllSatisfy(w => w.Currency.Should().NotBeNull());
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
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
    public async Task GetById_ShouldReturnArchivedWallet()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(w => w.Status = EntityStatus.Archived);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.AddRange(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallet.Id}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Id.Should().Be(scenario.wallet.Id);
    }

    [Fact]
    public async Task GetById_WithoutIncludeParameter_ShouldReturnWalletWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.Currency)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().NotBeNull();
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.Transactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.IncomeTransferTransactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include={nameof(Wallet.OutgoingTransferTransactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().BeNull();
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletId = Random.GetItems(scenario.wallets.Select(w => w.Id).ToArray(), 1).First();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{walletId}?include=" +
                                             $"{nameof(Wallet.Currency)}," +
                                             $"{nameof(Wallet.Transactions)}," +
                                             $"{nameof(Wallet.IncomeTransferTransactions)}," +
                                             $"{nameof(Wallet.OutgoingTransferTransactions)}");
        var walletFromResponse = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        walletFromResponse.Should().NotBeNull();
        walletFromResponse.Currency.Should().NotBeNull();
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();

        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = currency.Id,
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var created = await DbContext.Wallets.AsNoTracking()
            .Include(w => w.Currency)
            .Include(w => w.Transactions)
            .Include(w => w.IncomeTransferTransactions)
            .Include(w => w.OutgoingTransferTransactions)
            .FirstOrDefaultAsync(w => w.Id == createdId);
        created.Should().NotBeNull();
        created.Name.Should().Be(upsert.Name);
        created.Balance.Should().Be(upsert.Balance);
        created.IsPartOfGeneralBalance.Should().Be(upsert.IsPartOfGeneralBalance.Value);
        created.Status.Should().Be(EntityStatus.Active);
        created.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        created.ModifiedDate.Should().BeExactly(created.CreatedDate);
        created.Currency.Id.Should().Be(upsert.CurrencyId.Value);
        created.IncomeTransferTransactions.Should().BeNullOrEmpty();
        created.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Theory]
    [InlineData(1, TransactionType.Income)]
    [InlineData(-1, TransactionType.Expense)]
    public async Task Create_CreatesWalletWithInitialBalance_ShouldCreateCorrectionTransaction(int initialBalanceSign, TransactionType transactionType)
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000) * initialBalanceSign,
            IsPartOfGeneralBalance = false,
            CurrencyId = currency.Id,
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
        allTransactions[0].Type.Should().Be(transactionType);
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = string.Empty,
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = currency.Id
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = null,
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = currency.Id,
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = null,
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = currency.Id,
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = null,
            CurrencyId = currency.Id
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = null
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = Guid.NewGuid()
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
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            CurrencyId = Guid.Empty,
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
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
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
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
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
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
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
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
        updatedWallet.CurrencyId.Should().Be(upsert.CurrencyId.Value);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_UpdateWalletBalanceWithBiggerAmount_ShouldCreateIncomeCorrectionTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
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
        correctionTransaction.Type.Should().Be(TransactionType.Income);
        correctionTransaction.CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
    
    [Fact]
    public async Task Update_UpdateWalletBalanceWithSmallerAmount_ShouldCreateExpenseCorrectionTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
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
        correctionTransaction.Type.Should().Be(TransactionType.Expense);
        correctionTransaction.CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
    
    [Fact]
    public async Task Update_WhenWalletBalanceIsNotChanged_ShouldNotCreateAnyTransactions()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
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
    public async Task Update_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
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
    
    [Fact]
    public async Task Update_WithIncorrectCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
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
    
    #endregion
    
    #region DELETE TESTS

    [Theory]
    [InlineData(EntityStatus.Active)]
    [InlineData(EntityStatus.Archived)]
    public async Task HardDelete_WithCorrectData_ShouldDeleteWallet(EntityStatus walletStatus)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies(
            configureWallet: w => w.Status = walletStatus
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        deletedWallet.Should().BeNull();
    }
    
    [Theory]
    [InlineData(EntityStatus.Active)]
    [InlineData(EntityStatus.Archived)]
    public async Task HardDelete_WithCorrectData_ShouldDeleteRelatedTransactions(EntityStatus transactionStatus)
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions(
            configureTransaction: t => t.Status = transactionStatus,
            configureTransferTransaction: t => t.Status = transactionStatus
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletIdToDelete = scenario.wallets[0].Id;
        var expectedNumberOfTransactions = scenario.transactions.Count - scenario.transactions.Count(
            t => t.WalletId == walletIdToDelete || t.SourceWalletId == walletIdToDelete || t.TargetWalletId == walletIdToDelete);
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{walletIdToDelete}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedTransactions = await DbContext.Transactions.AsNoTracking().ToListAsync();
        deletedTransactions.Count.Should().Be(expectedNumberOfTransactions);
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteRelatedCurrency()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
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
    public async Task HardDelete_WhenTransferTransactionExists_ShouldUpdateTargetWalletBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.Add(scenario.transaction);
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
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.Add(scenario.transaction);
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
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
    
    #region ARCHIVE TESTS
    
    [Fact]
    public async Task Archive_WithCorrectData_ShouldArchiveWallet()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.DeleteAsync($"/api/wallet/archive/{scenario.wallet.Id}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var archivedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallet.Id);
        archivedWallet.Should().NotBeNull();
        archivedWallet.Status.Should().Be(EntityStatus.Archived);
    }
    
    [Fact]
    public async Task Archive_WithCorrectData_ShouldArchiveRelatedTransactions()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletsWithTransactions();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Transactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        var walletIdToArchive = scenario.wallets[0].Id;
    
        // Act
        var response = await Client.DeleteAsync($"/api/wallet/archive/{walletIdToArchive}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var archivedTransactions = await DbContext.Transactions
            .Where(t => t.WalletId == walletIdToArchive || t.SourceWalletId == walletIdToArchive || t.TargetWalletId == walletIdToArchive)
            .AsNoTracking().ToListAsync();
        archivedTransactions.Should().NotBeNullOrEmpty();
        archivedTransactions.Should().AllSatisfy(t => t.Status.Should().Be(EntityStatus.Archived));
    }
    
    [Fact]
    public async Task Archive_WhenTransferTransactionExists_ShouldUpdateTargetWalletBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var sourceWallet = scenario.wallets.First(w => w.Id == scenario.transaction.SourceWalletId);
        var targetWallet = scenario.wallets.First(w => w.Id == scenario.transaction.TargetWalletId);
        var expectedTargetWalletBalance = targetWallet.Balance - scenario.transaction.Amount;
        
        // Act
        var response = await Client.DeleteAsync($"/api/wallet/archive/{sourceWallet.Id}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedTargetWallet = await DbContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == targetWallet.Id);
        updatedTargetWallet.Should().NotBeNull();
        updatedTargetWallet.Balance.Should().Be(expectedTargetWalletBalance);
    }
    
    [Fact]
    public async Task Archive_WhenTransferTransactionExists_ShouldUpdateSourceWalletBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransferScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
        var sourceWallet = scenario.wallets.First(w => w.Id == scenario.transaction.SourceWalletId);
        var targetWallet = scenario.wallets.First(w => w.Id == scenario.transaction.TargetWalletId);
        var expectedSourceWalletBalance = sourceWallet.Balance + scenario.transaction.Amount;
        
        // Act
        var response = await Client.DeleteAsync($"/api/wallet/archive/{targetWallet.Id}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedSourceWallet = await DbContext.Wallets.AsNoTracking().FirstAsync(w => w.Id == sourceWallet.Id);
        updatedSourceWallet.Should().NotBeNull();
        updatedSourceWallet.Balance.Should().Be(expectedSourceWalletBalance);
    }
    
    [Fact]
    public async Task Archive_WithIncorrectId_ShouldReturnNotFoundResult() 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/wallet/archive/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Archive_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/wallet/archive/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion
}