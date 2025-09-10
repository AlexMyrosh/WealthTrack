using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Category;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("CategoryTests")]
    public class CategoryControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetAll_ShouldReturnsAllCategoriesWithParentAndChildren()
        {
            // Arrange
            var (parent, children) = DataFactory.CreateCategoryHierarchyScenario(2);
            DbContext.Categories.Add(parent);
            DbContext.Categories.AddRange(children);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/category");
            var models = await response.Content.ReadFromJsonAsync<List<CategoryDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            models.Should().NotBeNullOrEmpty();
            models.Should().HaveCount(3);
            models.Should().ContainSingle(c => c.Id == parent.Id && c.ChildCategories.Count == 2);
            models.Should().Contain(c => c.ParentCategory != null && c.ParentCategory.Id == parent.Id);
        }
        
        [Fact]
        public async Task GetById_WithIncludeAllRelatedEntities_ReturnsCategoryWithParentAndChildren()
        {
            // Arrange
            var (parent, children) = DataFactory.CreateCategoryHierarchyScenario(2);
            DbContext.Categories.Add(parent);
            DbContext.Categories.AddRange(children);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/category/{parent.Id}?include=ParentCategory,ChildCategories");
            var model = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(parent.Id);
            model.ParentCategory.Should().BeNull();
            model.ChildCategories.Should().NotBeNullOrEmpty();
            model.ChildCategories!.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsCategoryWithoutParentOrChildren()
        {
            // Arrange
            var (parent, children) = DataFactory.CreateCategoryHierarchyScenario(1);
            var child = children[0];
            DbContext.Categories.Add(parent);
            DbContext.Categories.Add(child);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/category/{child.Id}");
            var model = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(child.Id);
            model.ParentCategory.Should().BeNull();
            model.ChildCategories.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeParentCategory_ReturnsCategoryWithParentOnly()
        {
            // Arrange
            var (parent, children) = DataFactory.CreateCategoryHierarchyScenario(1);
            var child = children[0];
            DbContext.Categories.Add(parent);
            DbContext.Categories.Add(child);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/category/{child.Id}?include=ParentCategory");
            var model = await response.Content.ReadFromJsonAsync<CategoryDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(child.Id);
            model.ParentCategory.Should().NotBeNull();
            model.ParentCategory!.Id.Should().Be(parent.Id);
            model.ChildCategories.Should().BeNullOrEmpty();
        }
        
        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFound()
        {
            // Act
            var response = await Client.GetAsync($"/api/category/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_CreatesCategory()
        {
            // Arrange
            var upsert = new CategoryUpsertApiModel { Name = Guid.NewGuid().ToString(), Type = Shared.Enums.CategoryType.Expense };

            // Act
            var response = await Client.PostAsJsonAsync("/api/category/create", upsert);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var created = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == createdId);
            created.Should().NotBeNull();
            created!.Name.Should().Be(upsert.Name);
        }

        [Fact]
        public async Task Update_UpdatesCategoryName()
        {
            // Arrange
            var category = new Data.DomainModels.Category
            {
                Id = Guid.NewGuid(),
                Name = "Old",
                Type = Shared.Enums.CategoryType.Expense,
                Status = Shared.Enums.CategoryStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            DbContext.Categories.Add(category);
            await DbContext.SaveChangesAsync();

            var upsert = new CategoryUpsertApiModel { Name = "New Name" };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/category/update/{category.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updated = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == category.Id);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be(upsert.Name);
        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFound()
        {
            // Arrange
            var upsert = new CategoryUpsertApiModel { Name = "Updated" };
            var response = await Client.PutAsJsonAsync($"/api/category/update/{Guid.NewGuid()}", upsert);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HardDelete_DeletesCategoryAndItsChildrenOnly()
        {
            // Arrange
            var (parent, children) = DataFactory.CreateCategoryHierarchyScenario(2);
            DbContext.Categories.Add(parent);
            DbContext.Categories.AddRange(children);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.DeleteAsync($"/api/category/hard_delete/{parent.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var deletedParent = await DbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == parent.Id);
            var deletedChildren = DbContext.Categories.AsNoTracking().Select(c => children.Select(ch => ch.Id).Contains(c.Id)).ToList();
            deletedParent.Should().BeNull();
            deletedChildren.Should().BeNullOrEmpty();
        }
        
        [Fact]
        public async Task HardDelete_RelatedGoalsAreUpdated()
        {

        }
    }
}