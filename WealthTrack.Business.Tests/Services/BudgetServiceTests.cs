using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Tests.TestModels;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.Tests.Services
{
    public class BudgetServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly BudgetService _budgetService;

        public BudgetServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.BudgetRepository).Returns(_budgetRepositoryMock.Object);
            _budgetService = new BudgetService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateBudgetSuccessfully()
        {
            // Arrange
            var testDomainModel = TestBudgetModels.DomainModel;
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Budget>(testUpsertBusinessModel)).Returns(testDomainModel);
            _budgetRepositoryMock.Setup(r => r.CreateAsync(It.Is<Budget>(x=> x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _budgetService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            _mapperMock.Verify(m => m.Map<Budget>(testUpsertBusinessModel), Times.Once);
            _budgetRepositoryMock.Verify(r => r.CreateAsync(It.Is<Budget>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenModelIsNull()
        {
            // Arrange
            BudgetUpsertBusinessModel? model = null;

            // Act
            Func<Task> act = async () => await _budgetService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Budget>(It.IsAny<BudgetUpsertBusinessModel>()), Times.Never);
            _budgetRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenIdIsEmpty()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            Func<Task> act = async () => await _budgetService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenBudgetNotFound()
        {
            // Arrange
            var budgetId = Guid.NewGuid();
            _budgetRepositoryMock.Setup(repo => repo.GetByIdAsync(budgetId, "")).ReturnsAsync((Budget?)null);

            // Act
            var result = await _budgetService.GetByIdAsync(budgetId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedBudget_WhenBudgetExists()
        {
            // Arrange
            var testDetailsBusinessModel = TestBudgetModels.DetailsBusinessModel;
            var testDomainModel = TestBudgetModels.DomainModel;
            var budgetId = testDomainModel.Id;
            _budgetRepositoryMock.Setup(repo => repo.GetByIdAsync(budgetId, "")).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<BudgetDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _budgetService.GetByIdAsync(budgetId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoBudgetsExist()
        {
            // Arrange
            var emptyBudgetsList = new List<Budget>();
            _budgetRepositoryMock.Setup(repo => repo.GetAllAsync("")).ReturnsAsync(emptyBudgetsList);
            _mapperMock.Setup(m => m.Map<List<BudgetDetailsBusinessModel>>(emptyBudgetsList)).Returns(new List<BudgetDetailsBusinessModel>());

            // Act
            var result = await _budgetService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedBudgets_WhenBudgetsExist()
        {
            // Arrange
            var budgets = new List<Budget> { TestBudgetModels.DomainModel };
            var expectedBusinessModels = new List<BudgetDetailsBusinessModel>
            {
                TestBudgetModels.DetailsBusinessModel
            };

            _budgetRepositoryMock.Setup(repo => repo.GetAllAsync("")).ReturnsAsync(budgets);
            _mapperMock.Setup(m => m.Map<List<BudgetDetailsBusinessModel>>(budgets)).Returns(expectedBusinessModels);

            // Act
            var result = await _budgetService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeEquivalentTo(expectedBusinessModels);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _budgetService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenBudgetNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, "")).ReturnsAsync((Budget?)null);

            // Act
            Func<Task> act = async () => await _budgetService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenBudgetExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var testDomainModel = TestBudgetModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, "")).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map(testUpsertBusinessModel, testDomainModel));

            // Act
            await _budgetService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.BudgetRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _budgetService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task HardDeleteAsync_WhenBudgetNotFound_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.HardDeleteAsync(id)).ReturnsAsync((Budget?)null);

            // Act
            var result = await _budgetService.HardDeleteAsync(id);

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenBudgetExists_ShouldReturnTrueAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestBudgetModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.HardDeleteAsync(id)).ReturnsAsync(testDomainModel);

            // Act
            var result = await _budgetService.HardDeleteAsync(id);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _budgetRepositoryMock.Reset();
        }
    }
}