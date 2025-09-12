using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.Data.DomainModels;
using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("BudgetTests")]
public class BudgetControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
{
    // GET ALL tests

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_WithoutIncludeParameter_ReturnsAllBudgetsWithEmptyRelatedEntities(int numberOfBudgets)
    {
        // Arrange
        var scenario = DataFactory.CreateMultiBudgetsScenario(numberOfBudgets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetIds = scenario.budgets.Select(b => b.Id).ToList();
        
        // Act
        var response = await Client.GetAsync("/api/budget");
        var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allBudgets.Should().NotBeNullOrEmpty();
        allBudgets.Should().HaveCount(numberOfBudgets);
        allBudgets.Should().AllSatisfy(budget => budgetIds.Should().Contain(budget.Id));
        allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().BeNull());
        allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_WithIncludedCurrency_ReturnsAllBudgetsWithCurrencyOnly(int numberOfBudgets)
    {
        // Arrange
        var scenario = DataFactory.CreateMultiBudgetsScenario(numberOfBudgets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetIds = scenario.budgets.Select(b => b.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Currency)}");
        var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allBudgets.Should().NotBeNullOrEmpty();
        allBudgets.Should().HaveCount(numberOfBudgets);
        allBudgets.Should().AllSatisfy(budget => budgetIds.Should().Contain(budget.Id));
        allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
        allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_WithIncludedWallets_ReturnsAllBudgetsWithWalletsOnly(int numberOfBudgets)
    {
        // Arrange
        var scenario = DataFactory.CreateMultiBudgetsScenario(numberOfBudgets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetIds = scenario.budgets.Select(b => b.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Wallets)}");
        var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allBudgets.Should().NotBeNullOrEmpty();
        allBudgets.Should().HaveCount(numberOfBudgets);
        allBudgets.Should().AllSatisfy(budget => budgetIds.Should().Contain(budget.Id));
        allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().BeNull());
        allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().NotBeNullOrEmpty());
        allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().HaveCount(scenario.numberOfWallets));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_WithIncludedAllRelatedEntities_ReturnsAllBudgetsWithAllRelatedEntities(int numberOfBudgets)
    {
        // Arrange
        var scenario = DataFactory.CreateMultiBudgetsScenario(numberOfBudgets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetIds = scenario.budgets.Select(b => b.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
        var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allBudgets.Should().NotBeNullOrEmpty();
        allBudgets.Should().HaveCount(numberOfBudgets);
        allBudgets.Should().AllSatisfy(budget => budgetIds.Should().Contain(budget.Id));
        allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
        allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().NotBeNullOrEmpty());
        allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().HaveCount(scenario.numberOfWallets));
    }

    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateMultiBudgetsScenario(2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/budget?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // GET BY ID tests

    [Fact]
    public async Task GetById_WithoutIncludeParameter_ReturnsBudgetWithEmptyRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}");
        var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budget.Should().NotBeNull();
        budget.Id.Should().Be(scenario.budget.Id);
        budget.Currency.Should().BeNull();
        budget.Wallets.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithIncludedCurrency_ReturnsBudgetWithCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}?include={nameof(Budget.Currency)}");
        var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budget.Should().NotBeNull();
        budget.Id.Should().Be(scenario.budget.Id);
        budget.Currency.Should().NotBeNull();
        budget.Currency.Id.Should().Be(scenario.budget.CurrencyId);
        budget.Wallets.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetById_WithIncludedWallets_ReturnsBudgetWithWalletsOnly(int numberOfWallets)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetWithMultipleWalletsScenario(numberOfWallets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletIds = scenario.wallets.Select(w => w.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}?include={nameof(Budget.Wallets)}");
        var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budget.Should().NotBeNull();
        budget.Id.Should().Be(scenario.budget.Id);
        budget.Currency.Should().BeNull();
        budget.Wallets.Should().NotBeNullOrEmpty();
        budget.Wallets.Should().HaveCount(numberOfWallets);
        budget.Wallets.Should().AllSatisfy(wallet => walletIds.Should().Contain(wallet.Id));
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetById_WithIncludedAllRelatedEntities_ReturnsBudgetWithAllRelatedEntities(int numberOfWallets)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetWithMultipleWalletsScenario(numberOfWallets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletIds = scenario.wallets.Select(w => w.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
        var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budget.Should().NotBeNull();
        budget.Id.Should().Be(scenario.budget.Id);
        budget.Currency.Should().NotBeNull();
        budget.Currency.Id.Should().Be(scenario.budget.CurrencyId);
        budget.Wallets.Should().NotBeNullOrEmpty();
        budget.Wallets.Should().HaveCount(numberOfWallets);
        budget.Wallets.Should().AllSatisfy(wallet => walletIds.Should().Contain(wallet.Id));
    }

    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetWithMultipleWalletsScenario(2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetWithMultipleWalletsScenario(2);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // CREATE tests

    [Fact]
    public async Task Create_WithCorrectData_CreatesNewBudgetWithCorrectDefaultData()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();

        var budgetToCreate = new BudgetUpsertApiModel
        {
            Name = $"Test Budget + {Guid.NewGuid()}",
            CurrencyId = currency.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var createdBudget = await DbContext.Budgets.AsNoTracking().Include(b => b.Wallets).FirstOrDefaultAsync(budget => budget.Id == createdId);
        createdBudget.Should().NotBeNull();
        createdBudget.Name.Should().Be(budgetToCreate.Name);
        createdBudget.CurrencyId.Should().Be(budgetToCreate.CurrencyId.Value);
        createdBudget.OverallBalance.Should().Be(0M);
        createdBudget.Status.Should().Be(BudgetStatus.Active);
        createdBudget.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdBudget.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdBudget.Wallets.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Create_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", (BudgetUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();

        var budgetToCreate = new BudgetUpsertApiModel
        {
            Name = string.Empty,
            CurrencyId = currency.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullName_ReturnsBadRequest()
    {
        // Arrange
        var currency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(currency);
        await DbContext.SaveChangesAsync();

        var budgetToCreate = new BudgetUpsertApiModel
        {
            Name = null,
            CurrencyId = currency.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNullCurrencyId_ReturnsBadRequest()
    {
        // Arrange
        var budgetToCreate = new BudgetUpsertApiModel
        {
            Name = $"Test Budget + {Guid.NewGuid()}",
            CurrencyId = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectCurrencyId_ReturnsBadRequest()
    {
        // Arrange
        var budgetToCreate = new BudgetUpsertApiModel
        {
            Name = $"Test Budget + {Guid.NewGuid()}",
            CurrencyId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // UPDATE tests

    [Fact]
    public async Task Update_WithNewName_UpdatesBudgetNameOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            Name = $"Updated Budget Name + {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().Include(b=>b.Wallets).FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.Name.Should().Be(budgetToUpdate.Name);
        updatedBudget.ModifiedDate.Should().NotBe(updatedBudget.CreatedDate);
        updatedBudget.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedBudget.CurrencyId.Should().Be(scenario.budget.CurrencyId);
        updatedBudget.Wallets.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Update_WithNewCurrency_UpdatesCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var newCurrency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Currencies.Add(newCurrency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            CurrencyId = newCurrency.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedBudget = await DbContext.Budgets.AsNoTracking().Include(b=>b.Wallets).FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.Name.Should().Be(scenario.budget.Name);
        updatedBudget.ModifiedDate.Should().NotBe(updatedBudget.CreatedDate);
        updatedBudget.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedBudget.CurrencyId.Should().Be(newCurrency.Id);
        updatedBudget.Wallets.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Update_WithEmptyId_ReturnsBadRequestResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new BudgetUpsertApiModel
        {
            Name = $"Updated Budget Name + {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{Guid.Empty}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithWrongId_ReturnsNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var modelToUpdate = new BudgetUpsertApiModel
        {
            Name = $"Updated Budget Name + {Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{Guid.NewGuid()}", modelToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Update_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", (BudgetUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        var newCurrency = DataFactory.CreateCurrency();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Currencies.Add(newCurrency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            Name = string.Empty,
            CurrencyId = newCurrency.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectCurrencyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleBudgetScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            Name = $"Updated Budget Name + {Guid.NewGuid()}",
            CurrencyId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // DELETE tests

    [Fact]
    public async Task HardDelete_WithCorrectData_DeletesBudgetAndAllRelatedData()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithAllRelatedEntitiesScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.sourceWallet, scenario.targetWallet);
        DbContext.TransferTransactions.Add(scenario.transfer);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        
        var deletedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
        var deletedSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.sourceWallet.Id);
        var deletedTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.targetWallet.Id);
        var deletedTransferTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transfer.Id);
        var deletedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transaction.Id);
        var existingCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == scenario.currency.Id);

        deletedBudget.Should().BeNull();
        deletedSourceWallet.Should().BeNull();
        deletedTargetWallet.Should().BeNull();
        deletedTransferTransaction.Should().BeNull();
        deletedTransaction.Should().BeNull();
        existingCurrency.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithAllRelatedEntitiesScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.sourceWallet, scenario.targetWallet);
        DbContext.TransferTransactions.Add(scenario.transfer);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{Guid.NewGuid()}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var existingBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
        var existingSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.sourceWallet.Id);
        var existingTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.targetWallet.Id);
        var existingTransferTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transfer.Id);
        var existingTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transaction.Id);
        var existingCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == scenario.currency.Id);

        existingBudget.Should().NotBeNull();
        existingSourceWallet.Should().NotBeNull();
        existingTargetWallet.Should().NotBeNull();
        existingTransferTransaction.Should().NotBeNull();
        existingTransaction.Should().NotBeNull();
        existingCurrency.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithAllRelatedEntitiesScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.sourceWallet, scenario.targetWallet);
        DbContext.TransferTransactions.Add(scenario.transfer);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var existingBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
        var existingSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.sourceWallet.Id);
        var existingTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.targetWallet.Id);
        var existingTransferTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transfer.Id);
        var existingTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transaction.Id);
        var existingCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == scenario.currency.Id);

        existingBudget.Should().NotBeNull();
        existingSourceWallet.Should().NotBeNull();
        existingTargetWallet.Should().NotBeNull();
        existingTransferTransaction.Should().NotBeNull();
        existingTransaction.Should().NotBeNull();
        existingCurrency.Should().NotBeNull();
    }
}