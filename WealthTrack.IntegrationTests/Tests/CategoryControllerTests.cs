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
    #region GET ALL TESTS

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_ShouldReturnCorrectNumberOfCategories(int numberOfParents, int numberOfChildren)
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies(numberOfParents, numberOfChildren);
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var expectedNumberOfCategories = scenario.parents.Count;

        // Act
        var response = await Client.GetAsync("/api/category");
        var categoriesFromResponse = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoriesFromResponse.Should().NotBeNullOrEmpty();
        categoriesFromResponse.Should().HaveCount(expectedNumberOfCategories);
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_WithoutIncludeParameter_ShouldReturnParentCategoriesOnly(int numberOfParents, int numberOfChildren)
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies(numberOfParents, numberOfChildren);
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var parentCategoryIds = scenario.parents.Select(c => c.Id).ToList();

        // Act
        var response = await Client.GetAsync("/api/category");
        var categoriesFromResponse = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoriesFromResponse.Should().NotBeNullOrEmpty();
        categoriesFromResponse.Should().AllSatisfy(c => parentCategoryIds.Contains(c.Id));
        categoriesFromResponse.Should().AllSatisfy(b => b.ParentCategory.Should().BeNull());
        categoriesFromResponse.Should().AllSatisfy(b => b.ChildCategories.Should().BeNullOrEmpty());
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    public async Task GetAll_WithIncludedChildCategories_ShouldReturnCategoriesWithLoadingChildren(int numberOfParents, int numberOfChildren)
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies(numberOfParents, numberOfChildren);
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/category?include={nameof(Category.ChildCategories)}");
        var categoriesFromResponse = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoriesFromResponse.Should().NotBeNullOrEmpty();
        categoriesFromResponse.Should().HaveCount(numberOfParents);
        categoriesFromResponse.Should().AllSatisfy(c => c.ChildCategories.Count.Should().Be(numberOfChildren));
    }
    
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    public async Task GetAll_WithIncludedChildCategories_ShouldReturnCategoriesWithLoadingAllLayers(int numberOfLayers)
    {
        // Arrange
        var categories = DataFactory.CreateCategoriesChain(numberOfLayers);
        DbContext.Categories.AddRange(categories);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/category?include={nameof(Category.ChildCategories)}");
        var categoriesFromResponse = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoriesFromResponse.Should().NotBeNullOrEmpty();
        categoriesFromResponse.Should().HaveCount(1);
        
        var root = categoriesFromResponse.Single();
        root.ChildCategories.Should().ContainSingle();
        var current = root.ChildCategories.Single();
        for (int i = 2; i < numberOfLayers; i++)
        {
            current.ChildCategories.Should().ContainSingle();
            current = current.ChildCategories.Single();
        }
    }

    [Fact]
    public async Task GetAll_ShouldNotReturnSystemCategories()
    {
        // Arrange
        var scenario = DataFactory.CreateManyCategoryHierarchiesIncludingSystemType();
        DbContext.Categories.AddRange(scenario.regularChildren);
        DbContext.Categories.AddRange(scenario.regularParents);
        DbContext.Categories.AddRange(scenario.systemChildren);
        DbContext.Categories.AddRange(scenario.systemParents);
        await DbContext.SaveChangesAsync();
        var expectedNumberOfCategories = scenario.regularParents.Count;

        // Act
        var response = await Client.GetAsync("/api/category");
        var categoriesFromResponse = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoriesFromResponse.Should().HaveCount(expectedNumberOfCategories);
    }
    
    [Fact]
    public async Task GetAll_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies();
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("/api/category?include=SomeProperty");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET BY ID TESTS

    [Fact]
    public async Task GetById_ShouldReturnCategoryWithCorrectId()
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies();
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var categoryId = Random.GetItems(scenario.parents.Concat(scenario.children).Select(c => c.Id).ToArray(), 1)
            .First();

        // Act
        var response = await Client.GetAsync($"/api/category/{categoryId}");
        var categoryFromResponse = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoryFromResponse.Should().NotBeNull();
        categoryFromResponse.Id.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetById_ParentCategory_WithoutIncludeParameter_ShouldReturnCategoryWithoutLoadingChildCategories()
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies();
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var parentCategoryId = Random.GetItems(scenario.parents.Select(c => c.Id).ToArray(), 1).First();

        // Act
        var response = await Client.GetAsync($"/api/category/{parentCategoryId}");
        var categoryFromResponse = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoryFromResponse.Should().NotBeNull();
        categoryFromResponse.ChildCategories.Should().BeNullOrEmpty();
        categoryFromResponse.ParentCategory.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ChildCategory_WithoutIncludeParameter_ShouldReturnCategoryWithoutLoadingParentCategory()
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies();
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var childCategoryId = Random.GetItems(scenario.children.Select(c => c.Id).ToArray(), 1).First();

        // Act
        var response = await Client.GetAsync($"/api/category/{childCategoryId}");
        var categoryFromResponse = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoryFromResponse.Should().NotBeNull();
        categoryFromResponse.ParentCategory.Should().BeNull();
        categoryFromResponse.ChildCategories.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_ParentCategory_WithIncludedChildCategories_ShouldReturnCategoryWithChildrenLoadedOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies();
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var parentCategoryId = Random.GetItems(scenario.parents.Select(c => c.Id).ToArray(), 1).First();

        // Act
        var response =
            await Client.GetAsync($"/api/category/{parentCategoryId}?include={nameof(Category.ChildCategories)}");
        var categoryFromResponse = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoryFromResponse.Should().NotBeNull();
        categoryFromResponse.ChildCategories.Should().NotBeNullOrEmpty();
        categoryFromResponse.ParentCategory.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ChildCategory_WithIncludedParentCategory_ShouldReturnCategoryWithParentLoadedOnly()
    {
        // Arrange
        var scenario = DataFactory.CreateManyNotSystemCategoryHierarchies();
        DbContext.Categories.AddRange(scenario.parents);
        DbContext.Categories.AddRange(scenario.children);
        await DbContext.SaveChangesAsync();
        var childCategoryId = Random.GetItems(scenario.children.Select(c => c.Id).ToArray(), 1).First();

        // Act
        var response = await Client.GetAsync($"/api/category/{childCategoryId}?include={nameof(Category.ParentCategory)}");
        var categoryFromResponse = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        categoryFromResponse.Should().NotBeNull();
        categoryFromResponse.ParentCategory.Should().NotBeNull();
        categoryFromResponse.ChildCategories.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_CategoryWithSystemType_ShouldReturnNotFound()
    {
        // Arrange
        var systemCategory = DataFactory.CreateSystemCategory();
        DbContext.Categories.Add(systemCategory);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/category/{systemCategory.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithIncorrectIncludeParameter_ShouldReturnBadRequest()
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
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/category/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
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

    #endregion

    #region CREATE TESTS

    [Theory]
    [InlineData(OperationType.Expense)]
    [InlineData(OperationType.Income)]
    public async Task Create_WithCorrectData_ShouldCreateCategoryWithCorrectDefaultData(OperationType type)
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = type);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = type,
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
        createdCategory.CreatedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        createdCategory.ModifiedDate.Should().BeExactly(createdCategory.CreatedDate);
        createdCategory.ChildCategories.Should().BeNullOrEmpty();
        createdCategory.Transactions.Should().BeNullOrEmpty();
        createdCategory.Goals.Should().BeNullOrEmpty();
        var parentCategoryFromDb = await DbContext.Categories.AsNoTracking()
            .Include(category => category.ChildCategories)
            .FirstOrDefaultAsync(c => c.Id == parentCategory.Id);
        parentCategoryFromDb.Should().NotBeNull();
        parentCategoryFromDb.ChildCategories.Should().ContainSingle(c => c.Id == createdId);
    }

    [Fact]
    public async Task Create_WithNullBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", (CategoryUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = OperationType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = string.Empty,
            IconName = Guid.NewGuid().ToString(),
            Type = OperationType.Income,
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNullName_ShouldReturnBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = OperationType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = null,
            IconName = Guid.NewGuid().ToString(),
            Type = OperationType.Income,
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNullType_ShouldReturnBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = OperationType.Income);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = null,
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithIncorrectType_ShouldReturnBadRequest()
    {
        // Arrange
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = (OperationType)99
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithSystemParentCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p =>
        {
            p.IsSystem = true;
        });
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithIncorrectParentCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = OperationType.Income,
            ParentCategoryId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_WithEmptyParentCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = OperationType.Income,
            ParentCategoryId = Guid.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Create_WithDifferentTypeThanParentHas_ShouldReturnBadRequest(OperationType parentType, OperationType childType)
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = parentType);
        DbContext.Categories.Add(parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Name = $"Test Category + {Guid.NewGuid()}",
            IconName = Guid.NewGuid().ToString(),
            Type = childType,
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/category/create", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion

    #region UPDATE TESTS

    [Fact]
    public async Task Update_WithNewName_ShouldUpdateCategoryNameOnly()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
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
        updated.IsSystem.Should().Be(category.IsSystem);
        updated.CreatedDate.Should().BeExactly(category.CreatedDate);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updated.ParentCategoryId.Should().Be(category.ParentCategoryId);
    }

    [Fact]
    public async Task Update_WithNewIconName_ShouldUpdateCategoryIconOnly()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
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
        updated.IsSystem.Should().Be(category.IsSystem);
        updated.CreatedDate.Should().BeExactly(category.CreatedDate);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updated.ParentCategoryId.Should().Be(category.ParentCategoryId);
    }

    [Fact]
    public async Task Update_WithNewParentCategory_ShouldUpdateParentCategoryOnly()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        var parentCategory = DataFactory.CreateCategory(p => p.Type = category.Type);
        DbContext.Categories.AddRange(category, parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updated = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
        updated.Should().NotBeNull();
        updated.Name.Should().Be(category.Name);
        updated.IconName.Should().Be(category.IconName);
        updated.Type.Should().Be(category.Type);
        updated.IsSystem.Should().Be(category.IsSystem);
        updated.CreatedDate.Should().BeExactly(category.CreatedDate);
        updated.ModifiedDate.Should().NotBe(updated.CreatedDate);
        updated.ModifiedDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromMinutes(1));
        updated.ParentCategoryId.Should().Be(upsert.ParentCategoryId);
    }
    
    [Fact]
    public async Task Update_WithNullBody_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", (CategoryUpsertApiModel)null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Update_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
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
    public async Task Update_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
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
    public async Task Update_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
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
    public async Task Update_WithSystemParentId_ShouldReturnBadRequest()
    {
        // Arrange
        var systemParentCategory = DataFactory.CreateCategory(p =>
        {
            p.IsSystem = true;
        });
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        DbContext.Categories.Add(systemParentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            ParentCategoryId = systemParentCategory.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithIncorrectParentId_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
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
    
    [Fact]
    public async Task Update_WithEmptyParentId_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            ParentCategoryId = Guid.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(OperationType.Expense, OperationType.Income)]
    [InlineData(OperationType.Income, OperationType.Expense)]
    public async Task Update_WithNewType_ShouldReturnBadRequest(OperationType oldType, OperationType newType)
    {
        // Arrange
        var category = DataFactory.CreateCategory(c => c.Type = oldType);
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            Type = newType
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [InlineData(OperationType.Income, OperationType.Expense)]
    [InlineData(OperationType.Expense, OperationType.Income)]
    public async Task Update_WithNewParentIdOfDifferentType_ShouldReturnBadRequest(OperationType parentType, OperationType childType)
    {
        // Arrange
        var parentCategory = DataFactory.CreateCategory(p => p.Type = parentType);
        var childCategory = DataFactory.CreateCategory(c => c.Type = childType);
        DbContext.Categories.AddRange(childCategory, parentCategory);
        await DbContext.SaveChangesAsync();

        var upsert = new CategoryUpsertApiModel
        {
            ParentCategoryId = parentCategory.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/category/update/{childCategory.Id}", upsert);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    #endregion

    #region DELETE TESTS

    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldDeleteCategory()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedCategory = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
        deletedCategory.Should().BeNull();
    }
    
    [Fact]
    public async Task HardDelete_WithCorrectData_ShouldDeleteChildCategories()
    {
        // Arrange
        var scenario = DataFactory.CreateCategoryHierarchyWithSingleChildScenario();
        DbContext.Categories.AddRange(scenario.parent, scenario.child);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{scenario.parent.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deletedChildrenCategory = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.child.Id);
        deletedChildrenCategory.Should().BeNull();
    }

    [Fact]
    public async Task HardDelete_ShouldNotDeleteParentCategory()
    {
        // Arrange
        var scenario = DataFactory.CreateCategoryHierarchyWithSingleChildScenario();
        DbContext.Categories.AddRange(scenario.parent, scenario.child);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{scenario.child.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingParent = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == scenario.parent.Id);
        existingParent.Should().NotBeNull();
    }
    
    [Fact]
    public async Task HardDelete_ShouldNotDeleteRelatedTransactions()
    {
        // Arrange
        var scenario = DataFactory.CreateSingleTransactionScenario();
        DbContext.Currencies.Add(scenario.currency);
        DbContext.Budgets.Add(scenario.budget);
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
    public async Task HardDelete_ShouldNotDeleteRelatedGoals()
    {
        // Arrange
        var scenario = DataFactory.CreateManyGoalsWithDependencies(numberOfCategories: 1);
        DbContext.Categories.AddRange(scenario.categories);
        DbContext.Goals.AddRange(scenario.goals);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/category/hard_delete/{scenario.categories[0].Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var existingGoals = await DbContext.Goals.AsNoTracking().ToListAsync();
        existingGoals.Should().NotBeNullOrEmpty();
        existingGoals.Count.Should().Be(scenario.goals.Count);
    }
    
    [Fact]
    public async Task HardDelete_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/category/hard_delete/{Guid.NewGuid()}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HardDelete_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        var category = DataFactory.CreateCategory();
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/category/hard_delete/{Guid.Empty}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}