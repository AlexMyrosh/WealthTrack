using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.ExceptionServices;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.Data.DomainModels;
using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("BudgetTests")]
public class BudgetControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET ALL TESTS

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_ShouldReturnCorrectNumberOfBudgets(int numberOfBudgets)
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies(numberOfBudgets);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetIds = scenario.budgets.Select(b => b.Id).ToList();
        
        // Act
        var response = await Client.GetAsync("/api/budget");
        var budgetsFromResponse = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetsFromResponse.Should().NotBeNullOrEmpty();
        budgetsFromResponse.Should().HaveCount(numberOfBudgets);
        budgetsFromResponse.Should().AllSatisfy(b => budgetIds.Should().Contain(b.Id));
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_ShouldReturnActiveBudgetsOnly(int numberOfActiveBudgets, int numberOfArchivedBudgets)
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfActiveAndArchivedBudgets(numberOfActiveBudgets, numberOfArchivedBudgets);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.activeBudgets);
        DbContext.Budgets.AddRange(scenario.archivedBudgets);
        await DbContext.SaveChangesAsync();
        var activeBudgetIds = scenario.activeBudgets.Select(b => b.Id).ToList();
        
        // Act
        var response = await Client.GetAsync("/api/budget");
        var budgetsFromResponse = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetsFromResponse.Should().NotBeNullOrEmpty();
        budgetsFromResponse.Should().HaveCount(numberOfActiveBudgets);
        budgetsFromResponse.Should().AllSatisfy(b => activeBudgetIds.Should().Contain(b.Id));
    }
    
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ShouldReturnBudgetsWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/budget");
        var budgetsFromResponse = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetsFromResponse.Should().NotBeNullOrEmpty();
        budgetsFromResponse.Should().AllSatisfy(b => b.Currency.Should().BeNull());
        budgetsFromResponse.Should().AllSatisfy(b => b.Wallets.Should().BeNullOrEmpty());
    }

    [Fact]
    public async Task GetAll_WithIncludedCurrency_ShouldReturnBudgetsWithLoadingCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Currency)}");
        var budgetsFromResponse = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetsFromResponse.Should().NotBeNullOrEmpty();
        budgetsFromResponse.Should().AllSatisfy(b => b.Currency.Should().NotBeNull());
        budgetsFromResponse.Should().AllSatisfy(b => b.Wallets.Should().BeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncludedWallets_ShouldReturnBudgetsWithLoadingWalletsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletIds = scenario.wallets.Select(b => b.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Wallets)}");
        var budgetsFromResponse = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetsFromResponse.Should().NotBeNullOrEmpty();
        budgetsFromResponse.Should().AllSatisfy(b => b.Currency.Should().BeNull());
        budgetsFromResponse.Should().AllSatisfy(b => b.Wallets.Should().NotBeNullOrEmpty());
        budgetsFromResponse.Should().AllSatisfy(b => walletIds.Should().IntersectWith(b.Wallets.Select(w => w.Id)));
    }

    [Fact]
    public async Task GetAll_WithIncludedAllRelatedEntities_ShouldReturnBudgetsWithLoadingAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var walletIds = scenario.wallets.Select(b => b.Id).ToList();

        // Act
        var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
        var budgetsFromResponse = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetsFromResponse.Should().NotBeNullOrEmpty();
        budgetsFromResponse.Should().AllSatisfy(b => b.Currency.Should().NotBeNull());
        budgetsFromResponse.Should().AllSatisfy(b => b.Wallets.Should().NotBeNullOrEmpty());
        budgetsFromResponse.Should().AllSatisfy(b => walletIds.Should().IntersectWith(b.Wallets.Select(w => w.Id)));
    }

    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/budget?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET BY ID TESTS
    
    [Fact]
    public async Task GetById_ShouldReturnBudgetWithCorrectId()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetId = Random.GetItems(scenario.budgets.Select(b => b.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{budgetId}");
        var budgetFromResponse = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetFromResponse.Should().NotBeNull();
        budgetFromResponse.Id.Should().Be(budgetId);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnArchiveBudget()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies(b => b.Status = EntityStatus.Archived);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}");
        var budgetFromResponse = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetFromResponse.Should().NotBeNull();
        budgetFromResponse.Id.Should().Be(scenario.budget.Id);
    }

    [Fact]
    public async Task GetById_WithoutIncludeParameter_ShouldReturnBudgetWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetId = Random.GetItems(scenario.budgets.Select(b => b.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{budgetId}");
        var budgetFromResponse = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetFromResponse.Should().NotBeNull();
        budgetFromResponse.Currency.Should().BeNull();
        budgetFromResponse.Wallets.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithIncludedCurrency_ShouldReturnBudgetWithCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetId = Random.GetItems(scenario.budgets.Select(b => b.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{budgetId}?include={nameof(Budget.Currency)}");
        var budgetFromResponse = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetFromResponse.Should().NotBeNull();
        budgetFromResponse.Currency.Should().NotBeNull();
        budgetFromResponse.Wallets.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithIncludedWallets_ShouldReturnBudgetWithWalletsOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetId = Random.GetItems(scenario.budgets.Select(b => b.Id).ToArray(), 1).First();

        // Act
        var response = await Client.GetAsync($"/api/budget/{budgetId}?include={nameof(Budget.Wallets)}");
        var budgetFromResponse = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetFromResponse.Should().NotBeNull();
        budgetFromResponse.Currency.Should().BeNull();
        budgetFromResponse.Wallets.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedAllRelatedEntities_ShouldReturnBudgetWithAllRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetId = Random.GetItems(scenario.budgets.Select(b => b.Id).ToArray(), 1).First();

        // Act
        var response = await Client.GetAsync($"/api/budget/{budgetId}?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
        var budgetFromResponse = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        budgetFromResponse.Should().NotBeNull();
        budgetFromResponse.Currency.Should().NotBeNull();
        budgetFromResponse.Wallets.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        var budgetId = Random.GetItems(scenario.budgets.Select(b => b.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{budgetId}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateManyBudgetsWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.AddRange(scenario.budgets);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/budget/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CREATE TESTS

    [Fact]
    public async Task Create_WithCorrectData_ShouldCreateNewBudgetWithCorrectDefaultData()
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
        createdBudget.Status.Should().Be(EntityStatus.Active);
        createdBudget.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdBudget.ModifiedDate.Should().BeExactly(createdBudget.CreatedDate);
        createdBudget.Wallets.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Create_WithNullBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", (BudgetUpsertApiModel)null!);

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
    public async Task Create_WithNullName_ShouldReturnBadRequest()
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
    public async Task Create_WithNullCurrencyId_ShouldReturnBadRequest()
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
    public async Task Create_WithEmptyCurrencyId_ShouldReturnBadRequest()
    {
        // Arrange
        var budgetToCreate = new BudgetUpsertApiModel
        {
            Name = $"Test Budget + {Guid.NewGuid()}",
            CurrencyId = Guid.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectCurrencyId_ShouldReturnBadRequest()
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

    #endregion
    
    #region UPDATE TESTS

    [Fact]
    public async Task Update_WithNewName_ShouldUpdateBudgetNameOnly() 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
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
        var updatedBudget = await DbContext.Budgets.AsNoTracking().Include(b => b.Wallets).FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.Name.Should().Be(budgetToUpdate.Name);
        updatedBudget.OverallBalance.Should().Be(scenario.budget.OverallBalance);
        updatedBudget.CreatedDate.Should().BeExactly(scenario.budget.CreatedDate);
        updatedBudget.ModifiedDate.Should().NotBe(updatedBudget.CreatedDate);
        updatedBudget.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedBudget.Status.Should().Be(scenario.budget.Status);
        updatedBudget.CurrencyId.Should().Be(scenario.budget.CurrencyId);
        updatedBudget.Wallets.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Update_WithNewCurrency_ShouldUpdateCurrencyOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var newCurrency = DataFactory.CreateCurrency();
        DbContext.Currencies.AddRange(scenario.currency, newCurrency);
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
        var updatedBudget = await DbContext.Budgets.AsNoTracking().Include(b => b.Wallets).FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        updatedBudget.Should().NotBeNull();
        updatedBudget.Name.Should().Be(scenario.budget.Name);
        updatedBudget.OverallBalance.Should().Be(scenario.budget.OverallBalance);
        updatedBudget.CreatedDate.Should().BeExactly(scenario.budget.CreatedDate);
        updatedBudget.ModifiedDate.Should().NotBe(updatedBudget.CreatedDate);
        updatedBudget.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedBudget.Status.Should().Be(scenario.budget.Status);
        updatedBudget.CurrencyId.Should().Be(budgetToUpdate.CurrencyId.Value);
        updatedBudget.Wallets.Should().NotBeNullOrEmpty();
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
    public async Task Update_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
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
    public async Task Update_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
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
    public async Task Update_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        await DbContext.SaveChangesAsync();
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            Name = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

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
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            CurrencyId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

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
        
        var budgetToUpdate = new BudgetUpsertApiModel
        {
            CurrencyId = Guid.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", budgetToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE TESTS

    [Theory]
    [InlineData(EntityStatus.Active)]
    [InlineData(EntityStatus.Archived)]
    public async Task HardDelete_WithCorrectData_ShouldDeleteBudget(EntityStatus budgetStatus)
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies(
            configureBudget: b => b.Status = budgetStatus    
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        deletedBudget.Should().BeNull();
    }
    
    [Theory]
    [InlineData(EntityStatus.Active)]
    [InlineData(EntityStatus.Archived)]
    public async Task HardDelete_WithCorrectData_ShouldDeleteRelatedWallets(EntityStatus walletStatus)
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets(
            configureWallets: w => w.Status = walletStatus    
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedWallets = await DbContext.Wallets.AsNoTracking().ToListAsync();
        deletedWallets.Should().BeNullOrEmpty();
    }
    
    [Theory]
    [InlineData(EntityStatus.Active)]
    [InlineData(EntityStatus.Archived)]
    public async Task HardDelete_WithCorrectData_ShouldDeleteRelatedTransactions(EntityStatus transactionStatus)
    {
        // Arrange
        var scenario = DataFactory.CreateMixOfTransactionsScenario(
            configureTransaction: t => t.Status = transactionStatus,
            configureTransferTransaction: t => t.Status = transactionStatus
        );
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        DbContext.TransferTransactions.AddRange(scenario.transferTransactions);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedTransferTransactions = await DbContext.TransferTransactions.AsNoTracking().ToListAsync();
        var deletedTransactions = await DbContext.Transactions.AsNoTracking().ToListAsync();
        deletedTransferTransactions.Should().BeNullOrEmpty();
        deletedTransactions.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteCurrency()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.currency.Id);
        existingCurrency.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteCategory()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallet);
        DbContext.Transactions.AddRange(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCategory = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.category.Id);
        existingCategory.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteGoal()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionWithApplicableGoal();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == scenario.goal.Id);
        existingGoal.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{Guid.NewGuid()}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task HardDelete_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
    
    #region ARCHIVE TESTS
    
    [Fact]
    public async Task Archive_WithCorrectData_ShouldArchiveBudget()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/budget/archive/{scenario.budget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var archivedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == scenario.budget.Id);
        archivedBudget.Should().NotBeNull();
        archivedBudget.Status.Should().Be(EntityStatus.Archived);
    }
    
    [Fact]
    public async Task Archive_WithCorrectData_ShouldArchiveRelatedWallets()
    {
        // Arrange
        var scenario = DataFactory.CreateManyWallets();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.AddRange(scenario.wallets);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/budget/archive/{scenario.budget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var archivedWallets = await DbContext.Wallets.AsNoTracking().ToListAsync();
        archivedWallets.Should().NotBeNullOrEmpty();
        archivedWallets.Should().AllSatisfy(w => w.Status.Should().Be(EntityStatus.Archived));
    }
    
    [Fact]
    public async Task Archive_WithCorrectData_ShouldArchiveRelatedTransactions()
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
        var response = await Client.DeleteAsync($"/api/budget/archive/{scenario.budget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var archivedTransferTransactions = await DbContext.TransferTransactions.AsNoTracking().ToListAsync();
        var archivedTransactions = await DbContext.Transactions.AsNoTracking().ToListAsync();
        archivedTransferTransactions.Should().NotBeNullOrEmpty();
        archivedTransactions.Should().NotBeNullOrEmpty();
        archivedTransferTransactions.Should().AllSatisfy(t => t.Status.Should().Be(EntityStatus.Archived));
        archivedTransactions.Should().AllSatisfy(t => t.Status.Should().Be(EntityStatus.Archived));
    }
    
    [Fact]
    public async Task Archive_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/budget/archive/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Archive_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateBudgetWithDependencies();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/budget/archive/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion
}