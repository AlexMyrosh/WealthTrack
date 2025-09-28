using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Goal;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("GoalTests")]
public class GoalControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
{
    #region GET ALL TESTS

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_ShouldReturnCorrectNumberOfGoals(int numberOfGoals)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(numberOfGoals);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        var goalIds = scenario.goals.Select(b => b.Id).ToList();
            
        // Act
        var response = await Client.GetAsync("/api/goal");
        var goalsFromResponse = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalsFromResponse.Should().NotBeNullOrEmpty();
        goalsFromResponse.Should().HaveCount(numberOfGoals);
        goalsFromResponse.Should().AllSatisfy(g => goalIds.Should().Contain(g.Id));
    }
        
    [Fact]
    public async Task GetAll_WithoutIncludeParameter_ShouldReturnGoalsWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies();
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/goal");
        var goalsFromResponse = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalsFromResponse.Should().AllSatisfy(goal => goal.Categories.Should().BeNullOrEmpty());
    }

    [Fact]
    public async Task GetAll_WithIncludedCategories_ShouldReturnGoalsWithLoadingCategories()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies();
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
            
        // Act
        var response = await Client.GetAsync($"/api/goal?include={nameof(Goal.Categories)}");
        var goalsFromResponse = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalsFromResponse.Should().AllSatisfy(goal => goal.Categories.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies();
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/goal?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    #endregion
    
    #region GET BY ID TESTS

    [Fact]
    public async Task GetById_ShouldReturnGoalWithCorrectId()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{goal.Id}");
        var goalFromResponse = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalFromResponse.Should().NotBeNull();
        goalFromResponse.Id.Should().Be(goal.Id);
    }
    
    [Fact]
    public async Task GetById_WithoutIncludeParameter_ShouldReturnGoalWithoutLoadingRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies();
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        var goalId = Random.GetItems(scenario.goals.Select(g => g.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{goalId}");
        var goalFromResponse = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalFromResponse.Should().NotBeNull();
        goalFromResponse.Categories.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncludedCategories_ShouldReturnGoalWithCategories()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies();
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        var goalId = Random.GetItems(scenario.goals.Select(g => g.Id).ToArray(), 1).First();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{goalId}?include={nameof(Goal.Categories)}");
        var goalFromResponse = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalFromResponse.Should().NotBeNull();
        goalFromResponse.Categories.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{goal.Id}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        // Act
        var response = await Client.GetAsync($"/api/goal/{Guid.NewGuid()}");
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
    
    #region CREATE TESTS

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_WithCorrectData_ShouldCreateNewGoalWithCorrectDefaultData(OperationType type)
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = type);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}", 
            PlannedMoneyAmount = Random.Next(100,1000),
            Type = type,
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var createdGoal = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.Name.Should().Be(upsert.Name);
        createdGoal.PlannedMoneyAmount.Should().Be(upsert.PlannedMoneyAmount);
        createdGoal.ActualMoneyAmount.Should().Be(0M);
        createdGoal.Type.Should().Be(upsert.Type);
        createdGoal.StartDate.Should().Be(upsert.StartDate);
        createdGoal.EndDate.Should().Be(upsert.EndDate);
        createdGoal.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdGoal.ModifiedDate.Should().BeExactly(createdGoal.CreatedDate);
        createdGoal.Categories.Should().NotBeNullOrEmpty();
        createdGoal.Categories.Should().ContainSingle(c => c.Id == category.Id);
    }
    
    [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_WhenApplicableTransactionsExist_ShouldCreateGoalWithCorrectActualMoneyAmount(OperationType applicableType, OperationType notApplicableType) 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var category = DataFactory.CreateCategory(category => category.Type = applicableType);
        var applicableTransaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = applicableType;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        var notApplicableTransaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = notApplicableType;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.AddRange(applicableTransaction, notApplicableTransaction);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = Random.Next(100,1000),
            Type = applicableType,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            CategoryIds = [category.Id]
        };

        var expectedGoalActualMoneyAmount = applicableTransaction.Amount;
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.ActualMoneyAmount.Should().Be(expectedGoalActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_WhenApplicableTransactionsNotExist_WhenTypeIsDifferent_ShouldCreateGoalWithZeroActualMoneyAmount(OperationType goalType, OperationType transactionType) 
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var category = DataFactory.CreateCategory(category => category.Type = goalType);
        var transaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = transactionType;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        DbContext.Transactions.Add(transaction);
        await DbContext.SaveChangesAsync();
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = Random.Next(100,1000),
            Type = goalType,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            CategoryIds = [category.Id]
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.ActualMoneyAmount.Should().Be(0M);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_WhenApplicableTransactionsNotExist_WhenCategoryIsDifferent_ShouldCreateGoalWithZeroActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var category1 = DataFactory.CreateCategory(category => category.Type = type);
        var category2 = DataFactory.CreateCategory(category => category.Type = type);
        var transaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = type;
            t.CategoryId = category1.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(category1, category2);
        DbContext.Transactions.Add(transaction);
        await DbContext.SaveChangesAsync();
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = Random.Next(100,1000),
            Type = type,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            CategoryIds = [category2.Id]
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.ActualMoneyAmount.Should().Be(0M);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_WhenApplicableTransactionsNotExist_WhenTransactionDateIsBigger_ShouldCreateGoalWithZeroActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var category = DataFactory.CreateCategory(category => category.Type = type);
        var transaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = type;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        DbContext.Transactions.Add(transaction);
        await DbContext.SaveChangesAsync();
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = Random.Next(100,1000),
            Type = type,
            StartDate = transaction.TransactionDate.AddDays(-30),
            EndDate = transaction.TransactionDate.AddMinutes(-1),
            CategoryIds = [category.Id]
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.ActualMoneyAmount.Should().Be(0M);
    }
    
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_WhenApplicableTransactionsNotExist_WhenTransactionDateIsLess_ShouldCreateGoalWithZeroActualMoneyAmount(OperationType type)
    {
        var scenario = DataFactory.CreateSingleWalletWithDependencies();
        var category = DataFactory.CreateCategory(category => category.Type = type);
        var transaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = type;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(category);
        DbContext.Transactions.Add(transaction);
        await DbContext.SaveChangesAsync();
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = Random.Next(100,1000),
            Type = type,
            StartDate = transaction.TransactionDate.AddMinutes(1),
            EndDate = transaction.TransactionDate.AddDays(30),
            CategoryIds = [category.Id]
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.ActualMoneyAmount.Should().Be(0M);
    }
        
    [Fact]
    public async Task Create_WithNullBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", (GoalUpsertApiModel)null!);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = string.Empty,
            PlannedMoneyAmount = 1000M,
            Type = category.Type,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithNullName_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = null,
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithNullPlannedMoneyAmount_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = null,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithNegativePlannedMoneyAmount_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = -1M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullType_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = null,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_WithCategoryOfDifferentType_ShouldReturnBadRequest(OperationType categoryType, OperationType goalType)
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = categoryType);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = Random.Next(100, 1000),
            Type = goalType,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = category.Type,
            StartDate = null,
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullEndDate_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = null,
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithEndDateLessThanStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(30),
            EndDate = DateTimeOffset.UtcNow.AddDays(-30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WhenEndDateEqualsStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var goalDate = DateTimeOffset.UtcNow;
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = category.Type,
            StartDate = goalDate,
            EndDate = goalDate,
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectType_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = (OperationType)99,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithNullCategories_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyListOfCategories_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = []
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithCategoryOfEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [Guid.Empty]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectCategories_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [Guid.NewGuid()]
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
    
    #region UPDATE TESTS

    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WithNewName_ShouldUpdateGoalNameOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(
            numberOfGoals: 1, 
            numberOfCategories: 3,
            configureCategory: c => c.Type = type,
            configureGoal: g => g.Type = type
        );
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            Name = $"Updated Goal Name + {Guid.NewGuid()}"
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goals[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goals[0].Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.Name.Should().Be(upsert.Name);
        updatedGoal.PlannedMoneyAmount.Should().Be(scenario.goals[0].PlannedMoneyAmount);
        updatedGoal.ActualMoneyAmount.Should().Be(scenario.goals[0].ActualMoneyAmount);
        updatedGoal.Type.Should().Be(scenario.goals[0].Type);
        updatedGoal.StartDate.Should().BeExactly(scenario.goals[0].StartDate);
        updatedGoal.EndDate.Should().BeExactly(scenario.goals[0].EndDate);
        updatedGoal.CreatedDate.Should().BeExactly(scenario.goals[0].CreatedDate);
        updatedGoal.ModifiedDate.Should().NotBe(updatedGoal.CreatedDate);
        updatedGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedGoal.Categories.Count.Should().Be(scenario.categories.Count);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WithNewPlannedMoneyAmount_ShouldUpdateGoalPlannedMoneyAmountOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(
            numberOfGoals: 1, 
            numberOfCategories: 3,
            configureCategory: c => c.Type = type,
            configureGoal: g => g.Type = type
        );
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            PlannedMoneyAmount = Random.Next(100, 1000)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goals[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goals[0].Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.Name.Should().Be(scenario.goals[0].Name);
        updatedGoal.PlannedMoneyAmount.Should().Be(upsert.PlannedMoneyAmount);
        updatedGoal.ActualMoneyAmount.Should().Be(scenario.goals[0].ActualMoneyAmount);
        updatedGoal.Type.Should().Be(scenario.goals[0].Type);
        updatedGoal.StartDate.Should().BeExactly(scenario.goals[0].StartDate);
        updatedGoal.EndDate.Should().BeExactly(scenario.goals[0].EndDate);
        updatedGoal.CreatedDate.Should().BeExactly(scenario.goals[0].CreatedDate);
        updatedGoal.ModifiedDate.Should().NotBe(updatedGoal.CreatedDate);
        updatedGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedGoal.Categories.Count.Should().Be(scenario.categories.Count);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WithNewStartDate_ShouldUpdateGoalStartDateOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(
            numberOfGoals: 1, 
            numberOfCategories: 3,
            configureCategory: c => c.Type = type,
            configureGoal: g => g.Type = type
        );
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            StartDate = DateTimeOffset.UtcNow
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goals[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goals[0].Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.Name.Should().Be(scenario.goals[0].Name);
        updatedGoal.PlannedMoneyAmount.Should().Be(scenario.goals[0].PlannedMoneyAmount);
        updatedGoal.ActualMoneyAmount.Should().Be(scenario.goals[0].ActualMoneyAmount);
        updatedGoal.Type.Should().Be(scenario.goals[0].Type);
        updatedGoal.StartDate.Should().BeExactly(upsert.StartDate);
        updatedGoal.EndDate.Should().BeExactly(scenario.goals[0].EndDate);
        updatedGoal.CreatedDate.Should().BeExactly(scenario.goals[0].CreatedDate);
        updatedGoal.ModifiedDate.Should().NotBe(updatedGoal.CreatedDate);
        updatedGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedGoal.Categories.Count.Should().Be(scenario.categories.Count);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WithNewEndDate_ShouldUpdateGoalEndDateOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(
            numberOfGoals: 1, 
            numberOfCategories: 3,
            configureCategory: c => c.Type = type,
            configureGoal: g => g.Type = type
        );
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            EndDate = DateTimeOffset.UtcNow
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goals[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goals[0].Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.Name.Should().Be(scenario.goals[0].Name);
        updatedGoal.PlannedMoneyAmount.Should().Be(scenario.goals[0].PlannedMoneyAmount);
        updatedGoal.ActualMoneyAmount.Should().Be(scenario.goals[0].ActualMoneyAmount);
        updatedGoal.Type.Should().Be(scenario.goals[0].Type);
        updatedGoal.StartDate.Should().BeExactly(scenario.goals[0].StartDate);
        updatedGoal.EndDate.Should().BeExactly(upsert.EndDate);
        updatedGoal.CreatedDate.Should().BeExactly(scenario.goals[0].CreatedDate);
        updatedGoal.ModifiedDate.Should().NotBe(updatedGoal.CreatedDate);
        updatedGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedGoal.Categories.Count.Should().Be(scenario.categories.Count);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WithNewCategories_ShouldUpdateGoalCategoriesOnly(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(
            numberOfGoals: 1, 
            numberOfCategories: 3,
            configureCategory: c => c.Type = type,
            configureGoal: g => g.Type = type
        );
        var newCategory = DataFactory.CreateCategory(c => c.Type = scenario.goals[0].Type);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Categories.Add(newCategory);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [newCategory.Id]
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goals[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goals[0].Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.Name.Should().Be(scenario.goals[0].Name);
        updatedGoal.PlannedMoneyAmount.Should().Be(scenario.goals[0].PlannedMoneyAmount);
        updatedGoal.ActualMoneyAmount.Should().Be(scenario.goals[0].ActualMoneyAmount);
        updatedGoal.Type.Should().Be(scenario.goals[0].Type);
        updatedGoal.StartDate.Should().BeExactly(scenario.goals[0].StartDate);
        updatedGoal.EndDate.Should().BeExactly(scenario.goals[0].EndDate);
        updatedGoal.CreatedDate.Should().BeExactly(scenario.goals[0].CreatedDate);
        updatedGoal.ModifiedDate.Should().NotBe(updatedGoal.CreatedDate);
        updatedGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedGoal.Categories.Should().ContainSingle(c => c.Id == newCategory.Id);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WhenUpdateStartDateToIncludeTransaction_ShouldUpdateActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyTransactionsWithDependencies(3,
            configureCategory: c => c.Type = type,
            configureTransactions: t => t.Type = type
        );
        scenario.transactions[0].TransactionDate = DateTimeOffset.UtcNow;
        scenario.transactions[1].TransactionDate = DateTimeOffset.UtcNow.AddDays(-1);
        scenario.transactions[2].TransactionDate = DateTimeOffset.UtcNow.AddDays(-3);
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.Categories = [scenario.category];
            g.StartDate = DateTimeOffset.Now.AddMinutes(-1);
            g.EndDate = DateTimeOffset.Now.AddDays(30);
            g.ActualMoneyAmount = scenario.transactions[0].Amount;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();

        var expectedActualMoneyAmount = scenario.transactions.Sum(t => t.Amount);
        var upsert = new GoalUpsertApiModel
        {
            StartDate = scenario.transactions.Min(t => t.TransactionDate).AddMinutes(-1)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WhenUpdateEndDateToIncludeTransaction_ShouldUpdateActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyTransactionsWithDependencies(3,
            configureCategory: c => c.Type = type,
            configureTransactions: t => t.Type = type
        );
        scenario.transactions[0].TransactionDate = DateTimeOffset.UtcNow.AddDays(-15);
        scenario.transactions[1].TransactionDate = DateTimeOffset.UtcNow.AddDays(-5);
        scenario.transactions[2].TransactionDate = DateTimeOffset.UtcNow;
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.Categories = [scenario.category];
            g.StartDate = DateTimeOffset.Now.AddDays(-30);
            g.EndDate = DateTimeOffset.Now.AddDays(-10);
            g.ActualMoneyAmount = scenario.transactions[0].Amount;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();

        var expectedActualMoneyAmount = scenario.transactions.Sum(t => t.Amount);
        var upsert = new GoalUpsertApiModel
        {
            EndDate = scenario.transactions.Max(t => t.TransactionDate).AddMinutes(1)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WhenUpdateCategoryIdsToIncludeTransaction_ShouldUpdateActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyTransactionsWithDependencies(3,
            configureCategory: c => c.Type = type,
            configureTransactions: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var newCategory = DataFactory.CreateCategory(c => c.Type = type);
        scenario.transactions[1].CategoryId = newCategory.Id;
        scenario.transactions[2].CategoryId = newCategory.Id;
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.Categories = [scenario.category];
            g.StartDate = DateTimeOffset.Now.AddMinutes(-30);
            g.EndDate = DateTimeOffset.Now.AddDays(30);
            g.ActualMoneyAmount = scenario.transactions[0].Amount;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.AddRange(scenario.category, newCategory);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();
        var expectedActualMoneyAmount = scenario.transactions.Where(t => t.CategoryId == newCategory.Id).Sum(t => t.Amount);
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [newCategory.Id]
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
    
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WhenUpdateStartDateToExcludeTransaction_ShouldUpdateActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyTransactionsWithDependencies(3,
            configureCategory: c => c.Type = type,
            configureTransactions: t => t.Type = type
        );
        scenario.transactions[0].TransactionDate = DateTimeOffset.UtcNow;
        scenario.transactions[1].TransactionDate = DateTimeOffset.UtcNow.AddDays(-5);
        scenario.transactions[2].TransactionDate = DateTimeOffset.UtcNow.AddDays(-10);
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.Categories = [scenario.category];
            g.StartDate = DateTimeOffset.Now.AddDays(-30);
            g.EndDate = DateTimeOffset.Now.AddDays(30);
            g.ActualMoneyAmount = scenario.transactions.Sum(t => t.Amount);
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();
        var expectedActualMoneyAmount = scenario.transactions[0].Amount;
        var upsert = new GoalUpsertApiModel
        {
            StartDate = scenario.transactions[0].TransactionDate.AddMinutes(-1)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WhenUpdateEndDateToExcludeTransaction_ShouldUpdateActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyTransactionsWithDependencies(3,
            configureCategory: c => c.Type = type,
            configureTransactions: t => t.Type = type
        );
        scenario.transactions[0].TransactionDate = DateTimeOffset.UtcNow;
        scenario.transactions[1].TransactionDate = DateTimeOffset.UtcNow.AddDays(-5);
        scenario.transactions[2].TransactionDate = DateTimeOffset.UtcNow.AddDays(-10);
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.Categories = [scenario.category];
            g.StartDate = DateTimeOffset.Now.AddDays(-30);
            g.EndDate = DateTimeOffset.Now.AddDays(30);
            g.ActualMoneyAmount = scenario.transactions.Sum(t => t.Amount);
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(scenario.category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();

        var expectedActualMoneyAmount = scenario.transactions[2].Amount;
        var upsert = new GoalUpsertApiModel
        {
            EndDate = scenario.transactions[2].TransactionDate.AddMinutes(1)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WhenUpdateCategoryIdsToExcludeTransaction_ShouldUpdateActualMoneyAmount(OperationType type)
    {
        // Arrange
        var scenario = DataFactory.CreateManyTransactionsWithDependencies(3,
            configureCategory: c => c.Type = type,
            configureTransactions: t => {
                t.Type = type;
                t.TransactionDate = DateTimeOffset.UtcNow;
            });
        var newCategory = DataFactory.CreateCategory();
        scenario.transactions[1].CategoryId = newCategory.Id;
        scenario.transactions[2].CategoryId = newCategory.Id;
        var goal = DataFactory.CreateGoal(g => {
            g.Type = type;
            g.Categories = [scenario.category, newCategory];
            g.StartDate = DateTimeOffset.Now.AddMinutes(-30);
            g.EndDate = DateTimeOffset.Now.AddDays(30);
            g.ActualMoneyAmount = scenario.transactions.Sum(t => t.Amount);
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.AddRange(scenario.category, newCategory);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(scenario.transactions);
        await DbContext.SaveChangesAsync();
        var expectedActualMoneyAmount = scenario.transactions.Where(t => t.CategoryId == scenario.category.Id).Sum(t => t.Amount);
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [scenario.category.Id]
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updatedGoal.Should().NotBeNull();
        updatedGoal.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
    }
        
    [Fact]
    public async Task Update_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", (GoalUpsertApiModel)null!);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            Name = $"New Name + {Guid.NewGuid()}"
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{Guid.Empty}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Update_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            Name = $"New Name + {Guid.NewGuid()}"
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{Guid.NewGuid()}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
        
    [Fact]
    public async Task Update_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            Name = string.Empty
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithNegativePlannedMoneyAmount_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            PlannedMoneyAmount = -1M
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Theory]
    [InlineData(OperationType.Expense, OperationType.Income)]
    [InlineData(OperationType.Income, OperationType.Expense)]
    public async Task Update_WithNewType_ShouldReturnBadRequest(OperationType oldType, OperationType newType)
    {
        // Arrange
        var goal = DataFactory.CreateGoal(g => g.Type = oldType);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            Type = newType
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithNewEndDate_WhenEndDateIsLessThanStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            EndDate = goal.StartDate.AddMinutes(-1)
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithNewStartDate_WhenStartDateIsBiggerThanEndDate_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            StartDate = goal.EndDate.AddMinutes(1)
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(OperationType.Expense, OperationType.Income)]
    [InlineData(OperationType.Income, OperationType.Expense)]
    public async Task Update_WithNewCategoryOfDifferentType_ShouldReturnBadRequest(OperationType goalType, OperationType categoryType)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(
            numberOfGoals: 1,
            configureCategory: c => c.Type = goalType,
            configureGoal: g => g.Type = goalType
            );
        var newCategory = DataFactory.CreateCategory(p => p.Type = categoryType);
        DbContext.Goals.Add(scenario.goals[0]);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Categories.Add(newCategory);
        await DbContext.SaveChangesAsync();
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [newCategory.Id]
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goals[0].Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithNewStartDate_WhenStartDateEqualsEndDate_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            StartDate = goal.EndDate
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithNewEndDate_WhenEndDateEqualsStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            EndDate = goal.StartDate
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyListOfCategories_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = []
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithCategoryOfEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [Guid.Empty]
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectCategoryIds_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [Guid.NewGuid()]
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion

    #region DELETE TESTS

    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldDeletesGoal()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/goal/hard_delete/{goal.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        deletedGoal.Should().BeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldNotDeleteCategory()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies();
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/goal/hard_delete/{scenario.goals[0].Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingCategories = await DbContext.Categories.AsNoTracking().ToListAsync();
        existingCategories.Count.Should().Be(scenario.categories.Count);
    }
    
    [Fact]
    public async Task HardDelete_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/goal/hard_delete/{Guid.NewGuid()}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
        
    [Fact]
    public async Task HardDelete_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        // Act
        var response = await Client.DeleteAsync($"/api/goal/hard_delete/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}