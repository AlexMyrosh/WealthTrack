using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Business.Tests.TestModels;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.Tests.Services
{
    public class CategoryServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ICategoryService _categoryService;

        public CategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.CategoryRepository).Returns(_categoryRepositoryMock.Object);
            _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateCategorySuccessfully()
        {
            // Arrange
            var testDomainModel = TestCategoryModels.DomainModel;
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Category>(testUpsertBusinessModel)).Returns(testDomainModel);
            _categoryRepositoryMock.Setup(r => r.CreateAsync(It.Is<Category>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _categoryService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            _mapperMock.Verify(m => m.Map<Category>(testUpsertBusinessModel), Times.Once);
            _categoryRepositoryMock.Verify(r => r.CreateAsync(It.Is<Category>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenModelIsNull()
        {
            // Arrange
            CategoryUpsertBusinessModel? model = null;

            // Act
            Func<Task> act = async () => await _categoryService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Category>(It.IsAny<CategoryUpsertBusinessModel>()), Times.Never);
            _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenIdIsEmpty()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            Func<Task> act = async () => await _categoryService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryNotFound()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<string>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedCategory_WhenCategoryExists()
        {
            // Arrange
            var testDetailsBusinessModel = TestCategoryModels.DetailsBusinessModel;
            var testDomainModel = TestCategoryModels.DomainModel;
            var categoryId = testDomainModel.Id;
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<CategoryDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategoriesExist()
        {
            // Arrange
            var emptyCategoriesList = new List<Category>();
            _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyCategoriesList);
            _mapperMock.Setup(m => m.Map<List<CategoryDetailsBusinessModel>>(emptyCategoriesList)).Returns(new List<CategoryDetailsBusinessModel>());

            // Act
            var result = await _categoryService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedCategories_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<Category> { TestCategoryModels.DomainModel };
            var expectedBusinessModels = new List<CategoryDetailsBusinessModel>
            {
                TestCategoryModels.DetailsBusinessModel
            };

            _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(categories);
            _mapperMock.Setup(m => m.Map<List<CategoryDetailsBusinessModel>>(categories)).Returns(expectedBusinessModels);

            // Act
            var result = await _categoryService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeEquivalentTo(expectedBusinessModels);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var testDomainModel = TestCategoryModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map(testUpsertBusinessModel, testDomainModel));

            // Act
            await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CategoryRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _categoryService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task HardDeleteAsync_WhenCategoryNotFound_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.HardDeleteAsync(id)).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.HardDeleteAsync(id);

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenCategoryExists_ShouldReturnTrueAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestCategoryModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.HardDeleteAsync(id)).ReturnsAsync(testDomainModel);

            // Act
            var result = await _categoryService.HardDeleteAsync(id);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _categoryRepositoryMock.Reset();
        }
    }
}
