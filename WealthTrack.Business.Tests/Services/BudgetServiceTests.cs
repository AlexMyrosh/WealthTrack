using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Business.Tests.TestModels;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.Services
{
    public class BudgetServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IBudgetService _budgetService;

        public BudgetServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _walletServiceMock = new Mock<IWalletService>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.BudgetRepository).Returns(_budgetRepositoryMock.Object);
            _budgetService = new BudgetService(_unitOfWorkMock.Object, _walletServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNotNull_ShouldCreateBudgetSuccessfully()
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
            Assert.InRange(testDomainModel.CreatedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            Assert.InRange(testDomainModel.ModifiedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            Assert.Equal(testDomainModel.Status, BudgetStatus.Active);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            BudgetUpsertBusinessModel? model = null;

            // Act
            var act = async () => await _budgetService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Budget>(It.IsAny<BudgetUpsertBusinessModel>()), Times.Never);
            _budgetRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var act = async () => await _budgetService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<BudgetDetailsBusinessModel>(It.IsAny<Budget>()), Times.Never);
            _budgetRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenBudgetNotFound_ShouldReturnNull()
        {
            // Arrange
            var budgetId = Guid.NewGuid();
            _budgetRepositoryMock.Setup(repo => repo.GetByIdAsync(budgetId, It.IsAny<string>())).ReturnsAsync((Budget?)null);

            // Act
            var result = await _budgetService.GetByIdAsync(budgetId);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<BudgetDetailsBusinessModel>(It.IsAny<Budget>()), Times.Once);
            _budgetRepositoryMock.Verify(r => r.GetByIdAsync(budgetId, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenBudgetFound_ShouldReturnMappedBudget()
        {
            // Arrange
            var testDetailsBusinessModel = TestBudgetModels.DetailsBusinessModel;
            var testDomainModel = TestBudgetModels.DomainModel;
            var budgetId = testDomainModel.Id;
            _budgetRepositoryMock.Setup(repo => repo.GetByIdAsync(budgetId, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<BudgetDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _budgetService.GetByIdAsync(budgetId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
            _mapperMock.Verify(m => m.Map<BudgetDetailsBusinessModel>(testDomainModel), Times.Once);
            _budgetRepositoryMock.Verify(r => r.GetByIdAsync(budgetId, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoBudgetsFound_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyBudgetsList = new List<Budget>();
            _budgetRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyBudgetsList);
            _mapperMock.Setup(m => m.Map<List<BudgetDetailsBusinessModel>>(emptyBudgetsList)).Returns(new List<BudgetDetailsBusinessModel>());

            // Act
            var result = await _budgetService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _mapperMock.Verify(m => m.Map<List<BudgetDetailsBusinessModel>>(emptyBudgetsList), Times.Once);
            _budgetRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenBudgetsFound_ShouldReturnMappedBudgets()
        {
            // Arrange
            var budgets = new List<Budget> { TestBudgetModels.DomainModel };
            var expectedBusinessModels = new List<BudgetDetailsBusinessModel>
            {
                TestBudgetModels.DetailsBusinessModel
            };
            var expectedSize = expectedBusinessModels.Count;

            _budgetRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(budgets);
            _mapperMock.Setup(m => m.Map<List<BudgetDetailsBusinessModel>>(budgets)).Returns(expectedBusinessModels);

            // Act
            var result = await _budgetService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedSize);
            result.Should().BeEquivalentTo(expectedBusinessModels);
            _mapperMock.Verify(m => m.Map<List<BudgetDetailsBusinessModel>>(budgets), Times.Once);
            _budgetRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            var act = async () => await _budgetService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.BudgetRepository.Update(It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenBudgetNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Budget?)null);

            // Act
            var act = async () => await _budgetService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.BudgetRepository.Update(It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenBudgetFound_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestBudgetModels.UpsertBusinessModel;
            var testDomainModel = TestBudgetModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _budgetService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.BudgetRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            Assert.InRange(testDomainModel.ModifiedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var act = async () => await _budgetService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _budgetRepositoryMock.Verify(r => r.HardDeleteAsync(It.IsAny<Budget>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenBudgetNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Budget?)null);

            // Act
            var act = async () => await _budgetService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenBudgetFound_ShouldDeleteBudgetAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestBudgetModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _budgetService.HardDeleteAsync(id);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.BudgetRepository.HardDeleteAsync(testDomainModel), Times.Once);
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