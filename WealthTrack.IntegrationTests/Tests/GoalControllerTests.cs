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
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_ShouldReturnGoalsWithCorrectActualMoneyAmount(int numberOfGoals, int numberOfTransactions)
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithManyApplicableTransactions(numberOfGoals, numberOfTransactions);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/goal");
        var goalsFromResponse = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalsFromResponse.Should().NotBeNullOrEmpty();
        goalsFromResponse.Should().AllSatisfy(g => g.ActualMoneyAmount.Should().Be(CalculateActualMoneyAmount(scenario.goals.First(g2 => g2.Id == g.Id), scenario.transactions)));
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
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetById_ShouldReturnGoalsWithCorrectActualMoneyAmount(int numberOfTransactions)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleGoalWithManyTransactions(numberOfTransactions);
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Categories.Add(scenario.category);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        var expectedActualMoneyAmount = CalculateActualMoneyAmount(scenario.goal, scenario.transactions);
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{scenario.goal.Id}");
        var goalFromResponse = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goalFromResponse.Should().NotBeNull();
        goalFromResponse.ActualMoneyAmount.Should().Be(expectedActualMoneyAmount);
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
        createdGoal.Type.Should().Be(upsert.Type);
        createdGoal.StartDate.Should().Be(upsert.StartDate);
        createdGoal.EndDate.Should().Be(upsert.EndDate);
        createdGoal.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdGoal.ModifiedDate.Should().BeExactly(createdGoal.CreatedDate);
        createdGoal.Categories.Should().NotBeNullOrEmpty();
        createdGoal.Categories.Should().ContainSingle(c => c.Id == category.Id);
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
        updatedGoal.Type.Should().Be(scenario.goals[0].Type);
        updatedGoal.StartDate.Should().BeExactly(scenario.goals[0].StartDate);
        updatedGoal.EndDate.Should().BeExactly(scenario.goals[0].EndDate);
        updatedGoal.CreatedDate.Should().BeExactly(scenario.goals[0].CreatedDate);
        updatedGoal.ModifiedDate.Should().NotBe(updatedGoal.CreatedDate);
        updatedGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updatedGoal.Categories.Should().ContainSingle(c => c.Id == newCategory.Id);
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
    
    private decimal CalculateActualMoneyAmount(Goal goal, List<Transaction> transactions)
    {
        return transactions.Where(
            t => t.Type == goal.Type &&
                 t.CategoryId.HasValue &&
                 goal.Categories.Select(c => c.Id).Contains(t.CategoryId.Value) &&
                 t.TransactionDate >= goal.StartDate && t.TransactionDate <= goal.EndDate
        ).Sum(t => t.Amount);
    }
}