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
    // GET ALL tests
        
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_WithoutInclude_ReturnsAllGoalsWithEmptyRelatedEntities(int numberOfGoals)
    {
        // Arrange
        var scenario = DataFactory.CreateMultiGoalsScenario(numberOfGoals);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        var goalIds = scenario.goals.Select(b => b.Id).ToList();
            
        // Act
        var response = await Client.GetAsync("/api/goal");
        var allGoals = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allGoals.Should().NotBeNullOrEmpty();
        allGoals.Should().HaveCount(numberOfGoals);
        allGoals.Should().AllSatisfy(goal => goalIds.Should().Contain(goal.Id));
        allGoals.Should().AllSatisfy(goal => goal.Categories.Should().BeNullOrEmpty());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAll_WithIncludedCategories_ReturnsAllGoalsWithCategories(int numberOfGoals)
    {
        // Arrange
        var scenario = DataFactory.CreateMultiGoalsScenario(numberOfGoals);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        var goalIds = scenario.goals.Select(b => b.Id).ToList();
            
        // Act
        var response = await Client.GetAsync($"/api/goal?include={nameof(Goal.Categories)}");
        var allGoals = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allGoals.Should().NotBeNullOrEmpty();
        allGoals.Should().HaveCount(numberOfGoals);
        allGoals.Should().AllSatisfy(goal => goalIds.Should().Contain(goal.Id));
        allGoals.Should().AllSatisfy(goal => goal.Categories.Should().NotBeNullOrEmpty());
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateMultiGoalsScenario(2);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/goal?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // GET BY ID tests
        
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetById_WithIncludedCategories_ReturnsGoalWithCategories(int numberOfCategories)
    {
        // Arrange
        var scenario = DataFactory.CreateSingleGoalsWithMultipleCategoriesScenario(numberOfCategories);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        var categoryIds = scenario.categories.Select(b => b.Id).ToList();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{scenario.goal.Id}?include={nameof(Goal.Categories)}");
        var goal = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goal.Should().NotBeNull();
        goal.Id.Should().Be(scenario.goal.Id);
        goal.Categories.Should().NotBeNullOrEmpty();
        goal.Categories.Should().HaveCount(numberOfCategories);
        goal.Categories.Should().AllSatisfy(g => categoryIds.Should().Contain(g.Id));
    }
        
    [Fact]
    public async Task GetById_WithoutInclude_ReturnsGoalEmptyRelatedEntities()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleGoalsWithMultipleCategoriesScenario(3);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{scenario.goal.Id}");
        var goal = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        goal.Should().NotBeNull();
        goal.Id.Should().Be(scenario.goal.Id);
        goal.Categories.Should().BeNullOrEmpty();
    }
        
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleGoalsWithMultipleCategoriesScenario(3);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Categories.AddRange(scenario.categories);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{scenario.goal.Id}?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task GetById_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleGoalsWithMultipleCategoriesScenario(3);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Categories.AddRange(scenario.categories);
        await DbContext.SaveChangesAsync();
            
        // Act
        var response = await Client.GetAsync($"/api/goal/{Guid.NewGuid()}");
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
        
    [Fact]
    public async Task GetById_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleGoalsWithMultipleCategoriesScenario(3);
        DbContext.Goals.Add(scenario.goal);
        DbContext.Categories.AddRange(scenario.categories);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/goal/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    // // CREATE tests
        
    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async Task Create_WithCorrectData_CreatesNewGoalWithCorrectDefaultData(OperationType type)
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = type);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}", 
            PlannedMoneyAmount = 1000M,
            Type = type,
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category.Id],
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
        createdGoal.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdGoal.Categories.Should().NotBeNullOrEmpty();
    }
        
    [Fact]
    public async Task Create_WhenApplicableTransactionsExist_CreatesGoalWithCorrectActualMoneyAmount()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        var category1 = DataFactory.CreateCategory(category => category.Type = OperationType.Expense);
        var category2 = DataFactory.CreateCategory(category => category.Type = OperationType.Expense);
        var applicableTransaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Expense;
            t.CategoryId = category1.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        var notApplicableTransaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Income;
            t.CategoryId = category2.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.AddRange(category1, category2);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.AddRange(applicableTransaction, notApplicableTransaction);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CategoryIds = [category1.Id]
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
        createdGoal.Should().NotBeNull();
        createdGoal.ActualMoneyAmount.Should().Be(applicableTransaction.Amount);
    }
        
    [Fact]
    public async Task Create_WhenApplicableTransactionsNotExist_CreatesGoalWithZeroActualMoneyAmount()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        var category = DataFactory.CreateCategory(category => category.Type = OperationType.Expense);
        var notApplicableTransaction = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Income;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.UtcNow;
            t.WalletId = scenario.wallet.Id;
        });
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Transactions.Add(notApplicableTransaction);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Expense,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
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
    public async Task Create_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/goal/create", (GoalUpsertApiModel)null!);
            
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = string.Empty,
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
    public async Task Create_WithNullName_ReturnsBadRequest()
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
    public async Task Create_WithNullPlannedMoneyAmount_ReturnsBadRequest()
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
    public async Task Create_WithNegativePlannedMoneyAmount_ReturnsBadRequest()
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
    public async Task Create_WithDifferentTypeOfGoalAndRelatedCategories_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel 
        { 
            Name = $"Test Goal + {Guid.NewGuid()}",
            PlannedMoneyAmount = 1000M,
            Type = OperationType.Income,
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
    public async Task Create_WithNullType_ReturnsBadRequest()
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
        
    [Fact]
    public async Task Create_WithNullStartDate_ReturnsBadRequest()
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
    public async Task Create_WithNullEndDate_ReturnsBadRequest()
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
    public async Task Create_WithEndDateLessThanStartDate_ReturnsBadRequest()
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
    public async Task Create_WithIncorrectType_ReturnsBadRequest()
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
    public async Task Create_WithoutCategories_ReturnsBadRequest()
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
        
    // UPDATE tests
        
    [Fact]
    public async Task Update_WithNewName_UpdatesGoalNameOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleCategoryWithGoalScenario();
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            Name = $"Updated Goal Name + {Guid.NewGuid()}"
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == scenario.goal.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(upsert.Name);
        updated.PlannedMoneyAmount.Should().Be(scenario.goal.PlannedMoneyAmount);
        updated.Type.Should().Be(scenario.goal.Type);
        updated.StartDate.Should().Be(scenario.goal.StartDate);
        updated.EndDate.Should().Be(scenario.goal.EndDate);
        updated.Categories.Select(u => u.Id).Should().OnlyContain(id => id == scenario.category.Id);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
        
    [Fact]
    public async Task Update_WithNewPlannedMoneyAmount_UpdatesGoalPlannedMoneyAmountOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleCategoryWithGoalScenario();
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            PlannedMoneyAmount = Random.Next(100, 1000)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goal.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(scenario.goal.Name);
        updated.PlannedMoneyAmount.Should().Be(upsert.PlannedMoneyAmount);
        updated.Type.Should().Be(scenario.goal.Type);
        updated.StartDate.Should().Be(scenario.goal.StartDate);
        updated.EndDate.Should().Be(scenario.goal.EndDate);
        updated.Categories.Select(u => u.Id).Should().OnlyContain(id => id == scenario.category.Id);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
        
    [Fact]
    public async Task Update_WithNewStartDate_UpdatesGoalStartDateOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleCategoryWithGoalScenario();
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            StartDate = DateTimeOffset.UtcNow
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goal.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(scenario.goal.Name);
        updated.PlannedMoneyAmount.Should().Be(scenario.goal.PlannedMoneyAmount);
        updated.Type.Should().Be(scenario.goal.Type);
        updated.StartDate.Should().Be(upsert.StartDate);
        updated.EndDate.Should().Be(scenario.goal.EndDate);
        updated.Categories.Select(u => u.Id).Should().OnlyContain(id => id == scenario.category.Id);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
        
    [Fact]
    public async Task Update_WithNewEndDate_UpdatesGoalEndDateOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleCategoryWithGoalScenario();
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            EndDate = DateTimeOffset.UtcNow
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{scenario.goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == scenario.goal.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(scenario.goal.Name);
        updated.PlannedMoneyAmount.Should().Be(scenario.goal.PlannedMoneyAmount);
        updated.Type.Should().Be(scenario.goal.Type);
        updated.StartDate.Should().Be(scenario.goal.StartDate);
        updated.EndDate.Should().Be(upsert.EndDate);
        updated.Categories.Select(u => u.Id).Should().OnlyContain(id => id == scenario.category.Id);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
        
    [Fact]
    public async Task Update_WithNewCategories_UpdatesGoalCategoriesOnly()
    {
        // Arrange
        var goal = DataFactory.CreateGoal(g => g.Type = OperationType.Income);
        var oldCategory = DataFactory.CreateCategory(c =>
        {
            c.Goals = [goal];
            c.Type = OperationType.Income;
        });
        var newCategory = DataFactory.CreateCategory(c => c.Type = OperationType.Income);
        DbContext.Categories.AddRange(oldCategory, newCategory);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
        
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [newCategory.Id]
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id == goal.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(goal.Name);
        updated.PlannedMoneyAmount.Should().Be(goal.PlannedMoneyAmount);
        updated.Type.Should().Be(goal.Type);
        updated.StartDate.Should().Be(goal.StartDate);
        updated.EndDate.Should().Be(goal.EndDate);
        updated.Categories.Select(u => u.Id).Should().OnlyContain(id => id == newCategory.Id);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
        
    [Fact]
    public async Task Update_UpdateStartDate_ShouldUpdateActualMoneyAmount()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        var category = DataFactory.CreateCategory(category => category.Type = OperationType.Expense);
        var transaction1 = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Expense;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.Now;
            t.WalletId = scenario.wallet.Id;
        });
            
        var transaction2 = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Expense;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.Now.AddDays(-10);
            t.WalletId = scenario.wallet.Id;
        });
            
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = OperationType.Expense;
            g.Categories = [category];
            g.StartDate = DateTimeOffset.Now.AddDays(-5);
            g.EndDate = DateTimeOffset.Now.AddDays(5);
            g.ActualMoneyAmount = transaction1.Amount;
        });
            
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(transaction1, transaction2);
        await DbContext.SaveChangesAsync();
            
        var plannedActualMoneyAmount = transaction1.Amount +  transaction2.Amount;
        var upsert = new GoalUpsertApiModel
        {
            StartDate = transaction2.TransactionDate.AddDays(-1)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updated.Should().NotBeNull();
        updated.ActualMoneyAmount.Should().Be(plannedActualMoneyAmount);
    }
        
    [Fact]
    public async Task Update_UpdateEndDate_ShouldUpdateCurrentAmountToCorrectSum()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        var category = DataFactory.CreateCategory(category => category.Type = OperationType.Expense);
        var transaction1 = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Expense;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.Now.AddDays(-15);
            t.WalletId = scenario.wallet.Id;
        });
            
        var transaction2 = DataFactory.CreateTransaction(t =>
        {
            t.Type = OperationType.Expense;
            t.CategoryId = category.Id;
            t.TransactionDate = DateTimeOffset.Now.AddDays(-5);
            t.WalletId = scenario.wallet.Id;
        });
            
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = OperationType.Expense;
            g.Categories = [category];
            g.StartDate = DateTimeOffset.Now.AddDays(-20);
            g.EndDate = DateTimeOffset.Now.AddDays(-10);
            g.ActualMoneyAmount = transaction1.Amount;
        });
            
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.Add(category);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(transaction1, transaction2);
        await DbContext.SaveChangesAsync();
            
        var plannedActualMoneyAmount = transaction1.Amount +  transaction2.Amount;
        var upsert = new GoalUpsertApiModel
        {
            EndDate = transaction2.TransactionDate.AddDays(1)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updated.Should().NotBeNull();
        updated.ActualMoneyAmount.Should().Be(plannedActualMoneyAmount);
    }
        
    [Fact]
    public async Task Update_UpdateRelatedCategories_ShouldUpdateCurrentAmountToCorrectSum()
    {
        // Arrange
        var scenario = DataFactory.CreateWalletScenario();
        var category1 = DataFactory.CreateCategory(c => c.Type = OperationType.Expense);
        var category2 = DataFactory.CreateCategory(c => c.Type = OperationType.Expense);
            
        var transaction1 = DataFactory.CreateTransaction(t =>
        {
            t.CategoryId = category1.Id;
            t.TransactionDate = DateTimeOffset.Now;
            t.WalletId = scenario.wallet.Id;
            t.Type = OperationType.Expense;
        });
            
        var transaction2 = DataFactory.CreateTransaction(t =>
        {
            t.CategoryId = category2.Id;
            t.TransactionDate = DateTimeOffset.Now;
            t.WalletId = scenario.wallet.Id;
            t.Type = OperationType.Expense;
        });
            
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Categories = [category1];
            g.StartDate = DateTimeOffset.Now.AddDays(-5);
            g.EndDate = DateTimeOffset.Now.AddDays(5);
            g.ActualMoneyAmount = transaction1.Amount;
            g.Type = OperationType.Expense;
        });
            
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
        DbContext.Categories.AddRange(category1,  category2);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Goals.Add(goal);
        DbContext.Transactions.AddRange(transaction1, transaction2);
        await DbContext.SaveChangesAsync();
            
        var plannedActualMoneyAmount = transaction2.Amount;
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [category2.Id]
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
        updated.Should().NotBeNull();
        updated.ActualMoneyAmount.Should().Be(plannedActualMoneyAmount);
    }
        
    [Fact]
    public async Task Update_WithEmptyId_ReturnsBadRequest()
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
    public async Task Update_WithIncorrectId_ReturnsNotFound()
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
    public async Task Update_WithNullBody_ReturnsBadRequest()
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
    public async Task Update_WithEmptyName_ReturnsBadRequest()
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
        
    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Update_WithNewType_ReturnsBadRequest(OperationType categoryType)
    {
        // Arrange
        var goal = DataFactory.CreateGoal(g => g.Type = OperationType.Income);
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            Type = categoryType
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Update_WithNegativePlannedMoneyAmount_ReturnsBadRequest()
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
        
    [Fact]
    public async Task Update_WithEndDateLessThanStartDate_ReturnsBadRequest()
    {
        // Arrange
        var goal = DataFactory.CreateGoal();
        DbContext.Goals.Add(goal);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            EndDate = goal.StartDate.AddDays(-1)
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    [Fact]
    public async Task Update_WithCategoriesWithDifferentTypeThanGoalType_ReturnsBadRequest()
    {
        // Arrange
        var category1 = DataFactory.CreateCategory(p => p.Type = OperationType.Expense);
        var category2 = DataFactory.CreateCategory(p => p.Type = OperationType.Income);
        var goal = DataFactory.CreateGoal(g =>
        {
            g.Type = OperationType.Expense;
            g.Categories = [category1];
        });
        DbContext.Goals.Add(goal);
        DbContext.Categories.AddRange(category1, category2);
        await DbContext.SaveChangesAsync();
            
        var upsert = new GoalUpsertApiModel
        {
            CategoryIds = [category1.Id, category2.Id]
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
        
    // DELETE tests
        
    [Fact]
    public async Task HardDelete_DeletesGoalOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleCategoryWithGoalScenario();
        DbContext.Categories.Add(scenario.category);
        DbContext.Goals.Add(scenario.goal);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.DeleteAsync($"/api/goal/hard_delete/{scenario.goal.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == scenario.goal.Id);
        var existingCategory =  await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.category.Id);
        deletedGoal.Should().BeNull();
        existingCategory.Should().NotBeNull();
    }
        
    [Fact]
    public async Task HardDelete_WithWrongId_ReturnsNotFound()
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
    public async Task HardDelete_WithEmptyId_ReturnsBadRequest()
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
}