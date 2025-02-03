using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Business.Tests.TestModels;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.Services
{
    public class CategoryServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ICategoryService _categoryService;
        private readonly Mock<IEventPublisher> _eventPublisherMock;

        public CategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            _unitOfWorkMock.Setup(u => u.CategoryRepository).Returns(_categoryRepositoryMock.Object);
            _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapperMock.Object, _eventPublisherMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ModelIsNotNull_ShouldCreateCategorySuccessfully()
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
            Assert.InRange(testDomainModel.CreatedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            Assert.InRange(testDomainModel.ModifiedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            Assert.Equal(testDomainModel.Status, CategoryStatus.Active);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            CategoryUpsertBusinessModel? model = null;

            // Act
            var act = async () => await _categoryService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Category>(It.IsAny<CategoryUpsertBusinessModel>()), Times.Never);
            _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var act = async () => await _categoryService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<CategoryDetailsBusinessModel>(It.IsAny<Category>()), Times.Never);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryNotFound_ShouldReturnNull()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<string>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<CategoryDetailsBusinessModel>(It.IsAny<Category>()), Times.Once);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoryFound_ShouldReturnMappedCategory()
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
            _mapperMock.Verify(m => m.Map<CategoryDetailsBusinessModel>(It.IsAny<Category>()), Times.Once);
            _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoCategoriesFound_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyCategoriesList = new List<Category>();
            _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyCategoriesList);
            _mapperMock.Setup(m => m.Map<List<CategoryDetailsBusinessModel>>(emptyCategoriesList)).Returns(new List<CategoryDetailsBusinessModel>());

            // Act
            var result = await _categoryService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _mapperMock.Verify(m => m.Map<List<CategoryDetailsBusinessModel>>(emptyCategoriesList), Times.Once);
            _categoryRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenCategoriesFound_ShouldReturnMappedCategories()
        {
            // Arrange
            var categories = new List<Category> { TestCategoryModels.DomainModel };
            var expectedBusinessModels = new List<CategoryDetailsBusinessModel>
            {
                TestCategoryModels.DetailsBusinessModel
            };
            var expectedSize = expectedBusinessModels.Count;

            _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(categories);
            _mapperMock.Setup(m => m.Map<List<CategoryDetailsBusinessModel>>(categories)).Returns(expectedBusinessModels);

            // Act
            var result = await _categoryService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedSize);
            result.Should().BeEquivalentTo(expectedBusinessModels);
            _mapperMock.Verify(m => m.Map<List<CategoryDetailsBusinessModel>>(categories), Times.Once);
            _categoryRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            var act = async () => await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CategoryRepository.Update(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Category?)null);

            // Act
            var act = async () => await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CategoryRepository.Update(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryChanged_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var testDomainModel = TestCategoryModels.DomainModel;
            testUpsertBusinessModel.Type = CategoryType.Income;
            testDomainModel.Type = CategoryType.Expense;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            var act = async () => await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CategoryRepository.Update(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoryFound_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestCategoryModels.UpsertBusinessModel;
            var testDomainModel = TestCategoryModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _categoryService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CategoryRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var act = async () => await _categoryService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _categoryRepositoryMock.Verify(uow => uow.HardDelete(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(p=>p.PublishAsync(It.IsAny<CategoryDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenCategoryNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Category?)null);

            // Act
            var act = async () => await _categoryService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _categoryRepositoryMock.Verify(uow => uow.HardDelete(It.IsAny<Category>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<CategoryDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenCategoryFound_ShouldDeleteCategoryAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestCategoryModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _categoryService.HardDeleteAsync(id);

            // Assert
            _categoryRepositoryMock.Verify(uow => uow.HardDelete(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<CategoryDeletedEvent>()), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _categoryRepositoryMock.Reset();
            _eventPublisherMock.Reset();
        }
    }
}
