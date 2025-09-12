using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Category;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests;

[Collection("CategoryTests")]
public class CategoryControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
{
    // GET ALL tests
        
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_ShouldReturnsCategoriesWithParentAndChildren(int numberOfParents, int numberOfChildren)
    {
        // Arrange
        var (parents, children) = DataFactory.CreateCategoryHierarchyScenario(numberOfParents, numberOfChildren);
        DbContext.Categories.AddRange(parents);
        DbContext.Categories.AddRange(children);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/category");
        var allCategories = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allCategories.Should().NotBeNullOrEmpty();
        allCategories.Should().HaveCount(parents.Count + children.Count);
        allCategories.Should().NotContain(c => c.ParentCategory == null && c.ChildCategories.Count == 0);
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_ShouldReturnsNotSystemCategoriesOnly(int numberOfParents, int numberOfChildren)
    {
        // Arrange
        var (parents, children) = DataFactory.CreateCategoryHierarchyScenario(numberOfParents, numberOfChildren);
        var (systemParents, systemChildren) = DataFactory.CreateSystemCategoryHierarchyScenario(numberOfParents, numberOfChildren);
        DbContext.Categories.AddRange(parents);
        DbContext.Categories.AddRange(children);
        DbContext.Categories.AddRange(systemParents);
        DbContext.Categories.AddRange(systemChildren);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/category");
        var allCategories = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        allCategories.Should().NotBeNullOrEmpty();
        allCategories.Should().HaveCount(parents.Count + children.Count);
    }
    
    // GET BY ID tests
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetById_WithIncludedChildren_ReturnsCategoryWithChildrenLoaded(int numberOfChildren)
    {
        // Arrange
        var (parent, children) = DataFactory.CreateSingleCategoryHierarchyScenario(numberOfChildren);
        DbContext.Categories.AddRange(parent);
        DbContext.Categories.AddRange(children);
        await DbContext.SaveChangesAsync();
        var childrenIds = children.Select(c => c.Id);
    
        // Act
        var response = await Client.GetAsync($"/api/category/{parent.Id}?include={nameof(Category.ChildCategories)}");
        var category = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        category.Should().NotBeNull();
        category.Id.Should().Be(parent.Id);
        category.ChildCategories.Should().NotBeNullOrEmpty();
        category.ChildCategories.Count.Should().Be(numberOfChildren);
        category.ChildCategories.Should().AllSatisfy(child => childrenIds.Should().Contain(child.Id));
    }
    
    [Fact]
    public async Task GetById_WithIncludedParent_ReturnsCategoryWithParentLoaded()
    {
        // Arrange
        var (parent, child) = DataFactory.CreateCategoryHierarchyWithSingleChildScenario();
        DbContext.Categories.Add(parent);
        DbContext.Categories.Add(child);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/category/{child.Id}?include={nameof(Category.ParentCategory)}");
        var category = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        category.Should().NotBeNull();
        category.Id.Should().Be(child.Id);
        category.ParentCategory.Should().NotBeNull();
        category.ParentCategory.Id.Should().Be(parent.Id);
    }
    
    [Theory]
    [InlineData(3)]
    public async Task GetById_WithoutInclude_ReturnsCategoryWithoutChildren(int numberOfChildren)
    {
        // Arrange
        var (parent, children) = DataFactory.CreateSingleCategoryHierarchyScenario(numberOfChildren);
        DbContext.Categories.Add(parent);
        DbContext.Categories.AddRange(children);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/category/{parent.Id}");
        var category = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        category.Should().NotBeNull();
        category.Id.Should().Be(parent.Id);
        category.ChildCategories.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task GetById_WithoutInclude_ReturnsCategoryWithoutParent()
    {
        // Arrange
        var (parent, child) = DataFactory.CreateCategoryHierarchyWithSingleChildScenario();
        DbContext.Categories.Add(parent);
        DbContext.Categories.Add(child);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/category/{child.Id}");
        var category = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        category.Should().NotBeNull();
        category.Id.Should().Be(child.Id);
        category.ParentCategory.Should().BeNull();
    }
    
    [Fact]
    public async Task GetById_WithSystemCategoryType_ReturnsNotFound()
    {
        // Arrange
        var systemCategory = DataFactory.CreateCategory(c => c.Type = CategoryType.System);
        DbContext.Categories.Add(systemCategory);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/category/{systemCategory.Id}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.GetAsync($"/api/category/{category.Id}?include=SomeProperty");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetById_WithWrongId_ReturnsNotFound()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"/api/category/{Guid.NewGuid()}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    // CREATE tests
    
    [Fact]
    public async Task Create_WithCorrectData_CreatesCategoryWithCorrectDefaultData()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = CategoryType.Income,
            ParentCategoryId = parentCategory.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        createdId.Should().NotBe(Guid.Empty);
        var createdCategory = await DbContext.Categories.AsNoTracking()
            .Include(c => c.ChildCategories)
            .Include(c => c.Transactions)
            .Include(c => c.Goals)
            .FirstOrDefaultAsync(c => c.Id == createdId);
        createdCategory.Should().NotBeNull();
        createdCategory.Name.Should().Be(upsert.Name);
        createdCategory.IconName.Should().Be(upsert.IconName);
        createdCategory.Type.Should().Be(upsert.Type);
        createdCategory.ParentCategoryId.Should().Be(upsert.ParentCategoryId);
        createdCategory.Status.Should().Be(CategoryStatus.Active);
        createdCategory.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdCategory.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdCategory.ChildCategories.Should().BeNullOrEmpty();
        createdCategory.Transactions.Should().BeNullOrEmpty();
        createdCategory.Goals.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task Create_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", (CategoryUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = string.Empty,
            IconName = Guid.NewGuid().ToString(),
            Type = CategoryType.Income,
            ParentCategoryId = parentCategory.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithNullName_ReturnsBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = null,
            IconName = Guid.NewGuid().ToString(),
            Type = CategoryType.Income,
            ParentCategoryId = parentCategory.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithSystemCategory_ReturnsBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.System);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = CategoryType.System,
            ParentCategoryId = parentCategory.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithIncorrectParentCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = CategoryType.Income,
            ParentCategoryId = Guid.NewGuid()
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithDifferentTypeThanParent_ReturnsBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = CategoryType.Expense,
            ParentCategoryId = parentCategory.Id
        };
    
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // UPDATE tests
    
    [Fact]
    public async Task Update_WithNewName_UpdatesCategoryNameOnly()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"New Name + {Guid.NewGuid()}"
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(upsert.Name);
        updated.IconName.Should().Be(category.IconName);
        updated.Type.Should().Be(category.Type);
        updated.ParentCategoryId.Should().Be(category.ParentCategoryId);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
    
    [Fact]
    public async Task Update_WithNewIconName_UpdatesCategoryIconOnly()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            IconName = $"New Icon + {Guid.NewGuid()}"
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(category.Name);
        updated.IconName.Should().Be(upsert.IconName);
        updated.Type.Should().Be(category.Type);
        updated.ParentCategoryId.Should().Be(category.ParentCategoryId);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
    
    [Fact]
    public async Task Update_WithNewParentCategory_UpdatesParentCategoryOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateCategoryHierarchyWithSingleChildScenario();
        var parentCategory = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.AddRange(scenario.parent, scenario.child, parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            ParentCategoryId = parentCategory.Id
        };
    
        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{scenario.child.Id}", upsert);
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.child.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(scenario.child.Name);
        updated.IconName.Should().Be(scenario.child.IconName);
        updated.Type.Should().Be(scenario.child.Type);
        updated.ParentCategoryId.Should().Be(parentCategory.Id);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
    }
    
    [Fact]
    public async Task Update_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"New Name + {Guid.NewGuid()}"
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/category/update/{Guid.Empty}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectId_ReturnsNotFound()
    {
        // Arrange
        var category = DataFactory.CreateCategory(p => p.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"New Name + {Guid.NewGuid()}"
        };
        
        // Arrange
        var response = await Client.PutAsJsonAsync($"/api/category/update/{Guid.NewGuid()}", upsert);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", (CategoryUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Name = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyIconName_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            IconName = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithIncorrectParentId_ReturnsBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            ParentCategoryId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(CategoryType.Expense)]
    [InlineData(CategoryType.System)]
    public async Task Update_WithDifferentType_ReturnsBadRequest(CategoryType categoryType)
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = CategoryType.Income);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        
        var upsert = new CategoryUpsertApiModel
        {
            Type = categoryType
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // DELETE tests
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task HardDelete_DeletesCategoryAndItsChildrenOnly(int numberOfChildren)
    {
        // Arrange
        var (firstParent, secondParent, firstChildren, secondChildren) = DataFactory.CreateTwoPairsOfCategoryHierarchyScenario(numberOfChildren);
        DbContext.Categories.AddRange(firstParent, secondParent);
        DbContext.Categories.AddRange(firstChildren);
        DbContext.Categories.AddRange(secondChildren);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{firstParent.Id}");
    
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedParent = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == firstParent.Id);
        var deletedChildren = DbContext.Categories.AsNoTracking().Where(c => firstChildren.Select(x => x.Id).Contains(c.Id)).ToList();
        var existingParent = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.ParentCategoryId == secondParent.Id);
        var exisingChildren = DbContext.Categories.AsNoTracking().Where(c => secondChildren.Select(x => x.Id).Contains(c.Id)).ToList();
        
        deletedParent.Should().BeNull();
        deletedChildren.Should().BeNullOrEmpty();
        existingParent.Should().NotBeNull();
        exisingChildren.Should().NotBeNullOrEmpty();
        exisingChildren.Should().HaveCount(numberOfChildren);
    }
    
    [Fact]
    public async Task HardDelete_RelatedGoalStateIsUpdated()
    {
        // Arrange
        var scenario = DataFactory.CreateCategoriesWithGoalAndTransactionsScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.buddget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Transactions.AddRange(scenario.transactions);
        DbContext.Goals.AddRange(scenario.goal);
        await DbContext.SaveChangesAsync();
        var plannedActualMoneyAmount = scenario.goal.ActualMoneyAmount - scenario.categories[0].Transactions.Sum(t => t.Amount);
    
        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{scenario.categories[0].Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var goal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.goal.Id);
        goal.Should().NotBeNull();
        goal.ActualMoneyAmount.Should().Be(plannedActualMoneyAmount);
    }
    
    [Fact]
    public async Task HardDelete_ParentCategoryIsNotDeleted()
    {
        // Arrange
        var (parent, child) = DataFactory.CreateCategoryHierarchyWithSingleChildScenario();
        DbContext.Categories.AddRange(parent, child);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{child.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingParent = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == parent.Id);
        existingParent.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_RelatedTransactionsAreNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateCategoryWithTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.buddget);
        DbContext.Wallets.Add(scenario.wallet);
        DbContext.Categories.Add(scenario.category);
        DbContext.Transactions.Add(scenario.transaction);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{scenario.category.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.transaction.Id);
        existingTransaction.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_RelatedGoalsAreNotDeleted()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleCategoryWithGoalScenario();
        DbContext.Categories.AddRange(scenario.category);
        DbContext.Goals.AddRange(scenario.goal);
        await DbContext.SaveChangesAsync();
    
        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{scenario.category.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingGoal= await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.goal.Id);
        existingGoal.Should().NotBeNull();
    }
    
    [Theory]
    [InlineData(1, 4)]
    [InlineData(3, 8)]
    [InlineData(5, 12)]
    public async Task HardDelete_WithWrongId_ReturnsNotFound(int numberOfChildren, int totalNumberOfCategories)
    {
        // Arrange
        var (firstParent, secondParent, firstChildren, secondChildren) = DataFactory.CreateTwoPairsOfCategoryHierarchyScenario(numberOfChildren);
        DbContext.Categories.AddRange(firstParent, secondParent);
        DbContext.Categories.AddRange(firstChildren);
        DbContext.Categories.AddRange(secondChildren);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/category/hard_delete/{Guid.NewGuid()}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var existingCategory = await DbContext.Categories.AsNoTracking().ToListAsync();
        existingCategory.Should().NotBeNullOrEmpty();
        existingCategory.Should().HaveCount(totalNumberOfCategories);
    }
    
    [Theory]
    [InlineData(1, 4)]
    [InlineData(3, 8)]
    [InlineData(5, 12)]
    public async Task HardDelete_WithEmptyId_ReturnsBadRequest(int numberOfChildren, int totalNumberOfCategories)
    {
        // Arrange
        var (firstParent, secondParent, firstChildren, secondChildren) = DataFactory.CreateTwoPairsOfCategoryHierarchyScenario(numberOfChildren);
        DbContext.Categories.AddRange(firstParent, secondParent);
        DbContext.Categories.AddRange(firstChildren);
        DbContext.Categories.AddRange(secondChildren);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/category/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var existingCategory = await DbContext.Categories.AsNoTracking().ToListAsync();
        existingCategory.Should().NotBeNullOrEmpty();
        existingCategory.Should().HaveCount(totalNumberOfCategories);
    }
}