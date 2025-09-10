using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Goal;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("GoalTests")]
    public class GoalControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetAll_WithoutInclude_ReturnsGoalsWithoutRelatedEntities()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/goal");
            var models = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            models.Should().NotBeNullOrEmpty();
            models.Should().AllSatisfy(g =>
            {
                g.Categories.Should().BeNullOrEmpty();
            });
        }

        [Fact]
        public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsGoalsWithBudgetAndCategory()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/goal?include={nameof(Goal.Categories)}");
            var models = await response.Content.ReadFromJsonAsync<List<GoalDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            models.Should().NotBeNullOrEmpty();
            models.Should().AllSatisfy(g =>
            {
                g.Categories.Should().NotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task GetById_WithIncludeAllRelatedEntities_ReturnsGoalWithBudgetAndCategory()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/goal/{goal.Id}?include={nameof(Goal.Categories)}");
            var model = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(goal.Id);
            model.Categories.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsGoalWithoutRelatedEntities()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/goal/{goal.Id}");
            var model = await response.Content.ReadFromJsonAsync<GoalDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(goal.Id);
            model.Categories.Should().BeNullOrEmpty();
        }
        
        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFound()
        {
            // Act
            var response = await Client.GetAsync($"/api/goal/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_CreatesGoal()
        {
            // Arrange
            var (currency, budget, category, wallet, _, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            await DbContext.SaveChangesAsync();

            var upsert = new GoalUpsertApiModel 
            { 
                Name = "New Goal", 
                CategoryIds = [category.Id],
                PlannedMoneyAmount = 1000M,
                Type = GoalType.Expense,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(30)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var created = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
            created.Should().NotBeNull();
            created!.Name.Should().Be(upsert.Name);
        }

        [Fact]
        public async Task Create_WhenApplicableTransactionsExist_CreatesGoalWithNonZeroCurrentAmount()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, applicable, notApplicable) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Transactions.AddRange(applicable);
            DbContext.Transactions.AddRange(notApplicable);
            await DbContext.SaveChangesAsync();

            var plannedActualAmount = applicable.Sum(a => a.Amount);
            var upsert = new GoalUpsertApiModel 
            { 
                Name = "Goal with Transactions",
                CategoryIds = [category.Id],
                PlannedMoneyAmount = goal.PlannedMoneyAmount,
                Type = goal.Type,
                StartDate = goal.StartDate,
                EndDate = goal.EndDate,
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var created = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
            created.Should().NotBeNull();
            created!.ActualMoneyAmount.Should().Be(plannedActualAmount);
        }

        [Fact]
        public async Task Create_WhenApplicableTransactionsNotExist_CreatesGoalWithZeroCurrentAmount()
        {
            // Arrange
            var (currency, budget, category, wallet, _, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            await DbContext.SaveChangesAsync();

            var upsert = new GoalUpsertApiModel 
            { 
                Name = "Goal without Transactions", 
                CategoryIds = [category.Id],
                PlannedMoneyAmount = 1000M,
                Type = GoalType.Expense,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(30)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/goal/create", upsert);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var created = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == createdId);
            created.Should().NotBeNull();
            created!.ActualMoneyAmount.Should().Be(0);
        }

        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
        {
            // Act
            var response = await Client.PostAsJsonAsync("/api/goal/create", (GoalUpsertApiModel)null!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_UpdatesGoalName()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            var upsert = new GoalUpsertApiModel { Name = "Updated Goal Name" };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updated = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be(upsert.Name);
        }

        [Fact]
        public async Task Update_UpdateStartDate_ShouldUpdateCurrentAmountToCorrectSum()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, applicable, notApplicable) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            DbContext.Transactions.AddRange(applicable);
            DbContext.Transactions.AddRange(notApplicable);
            await DbContext.SaveChangesAsync();
            var plannedAmount = goal.ActualMoneyAmount + notApplicable.Where(x =>
                    x.TransactionDate > goal.StartDate.AddDays(-6) && x.TransactionDate < goal.EndDate)
                .Sum(t => t.Amount);

            var upsert = new GoalUpsertApiModel { StartDate = goal.StartDate.AddDays(-6) };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updated = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            updated.Should().NotBeNull();
            updated!.ActualMoneyAmount.Should().Be(plannedAmount);
        }

        [Fact]
        public async Task Update_UpdateEndDate_ShouldUpdateCurrentAmountToCorrectSum()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, applicable, notApplicable) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            DbContext.Transactions.AddRange(applicable);
            DbContext.Transactions.AddRange(notApplicable);
            await DbContext.SaveChangesAsync();
            var plannedAmount = goal.ActualMoneyAmount + notApplicable.Where(x=>x.TransactionDate > goal.EndDate).Sum(t => t.Amount);
            var upsert = new GoalUpsertApiModel
            {
                EndDate = DateTimeOffset.UtcNow.AddDays(-3)
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updated = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            updated.Should().NotBeNull();
            updated!.ActualMoneyAmount.Should().Be(plannedAmount);
        }
        
        [Fact]
        public async Task Update_UpdateRelatedCategories_ShouldUpdateCurrentAmountToCorrectSum()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, applicable, _) = DataFactory.CreateGoalWithTransactionsScenario();
            var newCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "New Category",
                Type = CategoryType.Expense,
                Status = CategoryStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Categories.Add(newCategory);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            DbContext.Transactions.AddRange(applicable);
            await DbContext.SaveChangesAsync();

            var upsert = new GoalUpsertApiModel { CategoryIds = [newCategory.Id] };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/goal/update/{goal.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updated = await DbContext.Goals.AsNoTracking().Include(goal => goal.Categories).FirstOrDefaultAsync(g => g.Id == goal.Id);
            updated.Should().NotBeNull();
            updated.Categories.Should().Contain(c => c.Id == newCategory.Id);
        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFound()
        {
            // Arrange
            var upsert = new GoalUpsertApiModel { Name = "Updated" };
            var response = await Client.PutAsJsonAsync($"/api/goal/update/{Guid.NewGuid()}", upsert);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HardDelete_DeletesGoalOnly()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.DeleteAsync($"/api/goal/hard_delete/{goal.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var deleted = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            deleted.Should().BeNull();
        }
    }
}