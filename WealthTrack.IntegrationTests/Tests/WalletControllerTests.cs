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
    // GET ALL tests
    
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ReturnsAllWalletsWithoutRelatedEntities()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/wallet");
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }

    [Fact]
    public async Task GetAll_WithIncludedCurrency_ReturnsAllWalletsWithCurrencyOnly()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Currency)}");
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().NotBeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedBudget_ReturnsAllWalletsWithBudgetOnly()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Budget)}");
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().NotBeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedTransactions_ReturnsAllWalletsWithTransactionsOnly()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.Transactions)}");
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().NotBeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedIncomeTransferTransactions_ReturnsAllWalletsWithIncomeAndOutgoingTransferTransactionsOnly()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.IncomeTransferTransactions)}");
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().NotBeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedOutgoingTransferTransactions_ReturnsAllWalletsWithOutgoingTransferTransactionsOnly()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/wallet?include={nameof(Wallet.OutgoingTransferTransactions)}");
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().BeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().BeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().NotBeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsAllWalletsWithAllRelatedEntities()
    {
        // Arrange
        var numberOfWallets = 2;
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
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
        var allWallets = await response.Content.ReadFromJsonAsync<List<WalletDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allWallets.Should().NotBeNullOrEmpty();
        allWallets.Should().HaveCount(numberOfWallets);
        allWallets.Should().AllSatisfy(w => w.Currency.Should().NotBeNull());
        allWallets.Should().AllSatisfy(w => w.Budget.Should().NotBeNull());
        allWallets.Should().AllSatisfy(w => w.Transactions.Should().NotBeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.IncomeTransferTransactions.Should().NotBeNullOrEmpty());
        allWallets.Should().AllSatisfy(w => w.OutgoingTransferTransactions.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
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

    // GET BY ID tests
    
    [Fact]
    public async Task GetById_WithoutIncludeParameter_ReturnsWalletWithEmptyRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().BeNull();
        wallet.Budget.Should().BeNull();
        wallet.Transactions.Should().BeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().BeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedCurrency_ReturnsWalletWithCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}?include={nameof(Wallet.Currency)}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().NotBeNull();
        wallet.Budget.Should().BeNull();
        wallet.Transactions.Should().BeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().BeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedBudget_ReturnsWalletWithBudgetOnly()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}?include={nameof(Wallet.Budget)}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().BeNull();
        wallet.Budget.Should().NotBeNull();
        wallet.Transactions.Should().BeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().BeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedTransactions_ReturnsWalletWithTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}?include={nameof(Wallet.Transactions)}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().BeNull();
        wallet.Budget.Should().BeNull();
        wallet.Transactions.Should().NotBeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().BeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedIncomeTransferTransactions_ReturnsWalletWithIncomeTransferTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}?include={nameof(Wallet.IncomeTransferTransactions)}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().BeNull();
        wallet.Budget.Should().BeNull();
        wallet.Transactions.Should().BeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedOutgoingTransferTransactions_ReturnsWalletWithOutgoingTransferTransactionsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}?include={nameof(Wallet.OutgoingTransferTransactions)}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().BeNull();
        wallet.Budget.Should().BeNull();
        wallet.Transactions.Should().BeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().BeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedAllRelatedEntities_ReturnsWalletWithAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallets[0].Id}?include=" +
                                             $"{nameof(Wallet.Budget)}," +
                                             $"{nameof(Wallet.Currency)}," +
                                             $"{nameof(Wallet.Transactions)}," +
                                             $"{nameof(Wallet.IncomeTransferTransactions)}," +
                                             $"{nameof(Wallet.OutgoingTransferTransactions)}");
        var wallet = await response.Content.ReadFromJsonAsync<WalletDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        wallet.Should().NotBeNull();
        wallet.Id.Should().Be(scenario.wallets[0].Id);
        wallet.Currency.Should().NotBeNull();
        wallet.Budget.Should().NotBeNull();
        wallet.Transactions.Should().NotBeNullOrEmpty();
        wallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        wallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{scenario.wallet.Id}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{Guid.NewGuid()}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetById_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/wallet/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // CREATE tests

    [Fact]
    public async Task Create_WithCorrectData_CreatesNewWalletWithCorrectDefaultData()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
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
        created.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        created.Currency.Id.Should().Be(upsert.CurrencyId.Value);
        created.Budget.Id.Should().Be(upsert.BudgetId.Value);
        created.Transactions.Should().BeNullOrEmpty();
        created.IncomeTransferTransactions.Should().BeNullOrEmpty();
        created.OutgoingTransferTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task Create_CreatesWalletAsPartOfGeneralBalance_UpdatesBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = true,
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
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
    public async Task Create_CreatesWalletAsNotPartOfGeneralBalance_BudgetBalanceNotUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = false,
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
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
    
    [Fact]
    public async Task Create_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", (WalletUpsertApiModel)null!);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = string.Empty,
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullName_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = null,
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullBalance_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = null,
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullIsPartOfGeneralBalance_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = null,
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullType_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = null,
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullCurrencyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = null,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectCurrencyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = Guid.NewGuid(),
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullBudgetId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = null
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectBudgetId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = Random
                .GetItems([WalletType.Cash, WalletType.CreditCard, WalletType.DebitCard, WalletType.SavingAccount], 1)
                .First(),
            CurrencyId = scenario.currency.Id,
            BudgetId = Guid.NewGuid()
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectType_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = $"Test Wallet + {Guid.NewGuid()}",
            Balance = Random.Next(100, 1000),
            IsPartOfGeneralBalance = Random.GetItems([true, false], 1).First(),
            Type = (WalletType)99,
            CurrencyId = scenario.currency.Id,
            BudgetId = scenario.budget.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/wallet/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // UPDATE tests
    
    [Fact]
    public async Task Update_WithNewName_UpdatesNameOnly()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
        
        var upsert = new WalletUpsertApiModel
        {
            Name = "Updated Name"
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
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.Balance.Should().Be(scenario.wallets[0].Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(scenario.wallets[0].IsPartOfGeneralBalance);
        updatedWallet.CreatedDate.Should().Be(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewBalance_UpdatesBalanceOnly()
    {
      // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
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
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.Balance.Should().Be(upsert.Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(scenario.wallets[0].IsPartOfGeneralBalance);
        updatedWallet.CreatedDate.Should().Be(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewIsPartOfGeneralBalance_UpdatesIsPartOfGeneralBalanceOnly()
    {
      // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
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
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.Balance.Should().Be(scenario.wallets[0].Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(upsert.IsPartOfGeneralBalance.Value);
        updatedWallet.CreatedDate.Should().Be(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(scenario.wallets[0].CurrencyId);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewCurrencyId_UpdatesCurrencyIdOnly()
    {
      // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        var newCurrency = DataFactory.CreateCurrency();
        DbContext.Currencies.AddRange(scenario.currency, newCurrency);
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
        updatedWallet.ModifiedDate.Should().NotBe(updatedWallet.CreatedDate);
        updatedWallet.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedWallet.Balance.Should().Be(scenario.wallets[0].Balance);
        updatedWallet.IsPartOfGeneralBalance.Should().Be(scenario.wallets[0].IsPartOfGeneralBalance);
        updatedWallet.CreatedDate.Should().Be(scenario.wallets[0].CreatedDate);
        updatedWallet.Status.Should().Be(scenario.wallets[0].Status);
        updatedWallet.Type.Should().Be(scenario.wallets[0].Type);
        updatedWallet.CurrencyId.Should().Be(upsert.CurrencyId.Value);
        updatedWallet.BudgetId.Should().Be(scenario.wallets[0].BudgetId);
        updatedWallet.Transactions.Should().NotBeNullOrEmpty();
        updatedWallet.IncomeTransferTransactions.Should().NotBeNullOrEmpty();
        updatedWallet.OutgoingTransferTransactions.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithNewIsPartOfGeneralBalance_UpdatesBudgetBalance()
    {

    }
    
    [Fact]
    public async Task Update_UpdateWalletBalanceWithBiggerAmount_ShouldCreateIncomeCorrectionTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var balanceCorrectionCategoryId = factory.Configuration["SystemCategories:BalanceCorrectionId"];
        var upsert = new WalletUpsertApiModel
        {
            Balance = Random.Next((int)scenario.wallet.Balance, (int)scenario.wallet.Balance + 1000)
        };
        var expectedTransactionAmount = upsert.Balance - scenario.wallet.Balance;
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var allTransactions = await DbContext.Transactions.AsNoTracking().ToListAsync();
        allTransactions.Should().HaveCount(1);
        allTransactions[0].Amount.Should().Be(expectedTransactionAmount);
        allTransactions[0].Type.Should().Be(OperationType.Income);
        allTransactions[0].CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
    
    [Fact]
    public async Task Update_UpdateWalletBalanceWithSmallerAmount_ShouldCreateExpenseCorrectionTransaction()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var balanceCorrectionCategoryId = factory.Configuration["SystemCategories:BalanceCorrectionId"];
        var upsert = new WalletUpsertApiModel
        {
            Balance = Random.Next((int)scenario.wallet.Balance - 1000, (int)scenario.wallet.Balance - 1)
        };
        var expectedTransactionAmount =  scenario.wallet.Balance - upsert.Balance;
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var allTransactions = await DbContext.Transactions.AsNoTracking().ToListAsync();
        allTransactions.Should().HaveCount(1);
        allTransactions[0].Amount.Should().Be(expectedTransactionAmount);
        allTransactions[0].Type.Should().Be(OperationType.Expense);
        allTransactions[0].CategoryId.ToString().Should().Be(balanceCorrectionCategoryId);
    }
        
    [Fact]
    public async Task Update_UpdateWalletBalance_WhenWalletIsPartOfGeneralBalance_ShouldUpdateBudgetBalance()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario(w => w.IsPartOfGeneralBalance = true);
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
        var scenario = DataFactory.CreateWalletScenario(w => w.IsPartOfGeneralBalance = false);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        var upsert = new WalletUpsertApiModel
        {
            Balance = Random.Next(100, 1000)
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(scenario.budget.OverallBalance);
    }
    
    [Fact]
    public async Task Update_WithEmptyId_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
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
    public async Task Update_WithIncorrectId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
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
    public async Task Update_WithNullBody_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{Guid.Empty}", (WalletUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
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
    [InlineData(WalletType.Cash)]
    [InlineData(WalletType.CreditCard)]
    [InlineData(WalletType.DebitCard)]
    [InlineData(WalletType.SavingAccount)]
    public async Task Update_WithNewType_ReturnsBadRequestResult(WalletType walletType)
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new WalletUpsertApiModel
        {
            Type = walletType
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/wallet/update/{scenario.wallet.Id}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectCurrencyId_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
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
    public async Task Update_WithNewBudget_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        var newBudget = DataFactory.CreateBudget();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
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
    
    // DELETE tests
    
    [Fact]
    public async Task HardDelete_WithCorrectData_DeletesWalletWithRelatedTransactionsAndTransferTransactions()
    {
        // Arrange
        var scenario = DataFactory.CreatePairOfWalletHierarchyWithTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallets[0].Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == scenario.wallets[0].Id);
        var deletedTransactions = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.WalletId == scenario.wallets[0].Id);
        var deletedTransferTransactions = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t =>
            t.SourceWalletId == scenario.wallets[0].Id || t.TargetWalletId == scenario.wallets[0].Id);
        deletedWallet.Should().BeNull();
        deletedTransactions.Should().BeNull();
        deletedTransferTransactions.Should().BeNull();
    }
    
    [Fact]
    public async Task HardDelete_RelatedCurrencyIsNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCurrency = DbContext.Currencies.AsNoTracking().FirstOrDefault(c => c.Id == scenario.wallet.CurrencyId);
        existingCurrency.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_RelatedBudgetIsNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingBudget = DbContext.Budgets.AsNoTracking().FirstOrDefault(c => c.Id == scenario.wallet.BudgetId);
        existingBudget.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WhenWalletWasAsPartOfGeneralBalance_BudgetBalanceUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario(w => w.IsPartOfGeneralBalance = true);
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
    public async Task HardDelete_WhenWalletWasNotAsPartOfGeneralBalance_BudgetBalanceNotUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario(w => w.IsPartOfGeneralBalance = false);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
    
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.OverallBalance.Should().Be(scenario.budget.OverallBalance);
    }
    
    [Fact]
    public async Task HardDelete_GoalsOfRelatedTransactionsAreUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateTransactionsHierarchyWithGoalScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        var expectedGoalActualMoneyAmount = scenario.goal.ActualMoneyAmount - scenario.transactions.Sum(t => t.Amount);
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{scenario.wallet.Id}");
    
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstAsync(b => b.Id == scenario.goal.Id);
        updatedGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Fact]
    public async Task HardDelete_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
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
    public async Task HardDelete_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/wallet/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}