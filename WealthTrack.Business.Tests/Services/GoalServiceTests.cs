using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Business.Tests.TestModels;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.Tests.Services
{
    public class GoalServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGoalRepository> _goalRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IGoalService _goalService;

        public GoalServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _goalRepositoryMock = new Mock<IGoalRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.GoalRepository).Returns(_goalRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.CategoryRepository).Returns(_categoryRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.WalletRepository).Returns(_walletRepositoryMock.Object);
            _goalService = new GoalService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateGoalSuccessfully()
        {
            // Arrange
            var testDomainModel = TestGoalModels.DomainModel;
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var testCategoryDomainModel = TestCategoryModels.DomainModel;
            var testWalletDomainModel = TestWalletModels.DomainModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Goal>(testUpsertBusinessModel)).Returns(testDomainModel);
            _goalRepositoryMock.Setup(r => r.CreateAsync(It.Is<Goal>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(testCategoryDomainModel);
            _walletRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(testWalletDomainModel);

            // Act
            var result = await _goalService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            Assert.Contains(testCategoryDomainModel, testDomainModel.Categories);
            Assert.Contains(testWalletDomainModel, testDomainModel.Wallets);
            _mapperMock.Verify(m => m.Map<Goal>(testUpsertBusinessModel), Times.Once);
            _goalRepositoryMock.Verify(r => r.CreateAsync(It.Is<Goal>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenModelIsNull()
        {
            // Arrange
            GoalUpsertBusinessModel? model = null;

            // Act
            Func<Task> act = async () => await _goalService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Goal>(It.IsAny<GoalUpsertBusinessModel>()), Times.Never);
            _goalRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Goal>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenIdIsEmpty()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            Func<Task> act = async () => await _goalService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenGoalNotFound()
        {
            // Arrange
            var goalId = Guid.NewGuid();
            _goalRepositoryMock.Setup(repo => repo.GetByIdAsync(goalId, It.IsAny<string>())).ReturnsAsync((Goal?)null);

            // Act
            var result = await _goalService.GetByIdAsync(goalId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedGoal_WhenGoalExists()
        {
            // Arrange
            var testDetailsBusinessModel = TestGoalModels.DetailsBusinessModel;
            var testDomainModel = TestGoalModels.DomainModel;
            var goalId = testDomainModel.Id;
            _goalRepositoryMock.Setup(repo => repo.GetByIdAsync(goalId, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<GoalDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _goalService.GetByIdAsync(goalId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGoalsExist()
        {
            // Arrange
            var emptyGoalsList = new List<Goal>();
            _goalRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyGoalsList);
            _mapperMock.Setup(m => m.Map<List<GoalDetailsBusinessModel>>(emptyGoalsList)).Returns(new List<GoalDetailsBusinessModel>());

            // Act
            var result = await _goalService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedGoals_WhenGoalsExist()
        {
            // Arrange
            var goals = new List<Goal> { TestGoalModels.DomainModel };
            var expectedBusinessModels = new List<GoalDetailsBusinessModel>
            {
                TestGoalModels.DetailsBusinessModel
            };

            _goalRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(goals);
            _mapperMock.Setup(m => m.Map<List<GoalDetailsBusinessModel>>(goals)).Returns(expectedBusinessModels);

            // Act
            var result = await _goalService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeEquivalentTo(expectedBusinessModels);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _goalService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenGoalNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Goal?)null);

            // Act
            Func<Task> act = async () => await _goalService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenGoalExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var testDomainModel = TestGoalModels.DomainModel;
            var testCategoryDomainModel = TestCategoryModels.DomainModel;
            var testWalletDomainModel = TestWalletModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map(testUpsertBusinessModel, testDomainModel));
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(testCategoryDomainModel);
            _walletRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(testWalletDomainModel);

            // Act
            await _goalService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            Assert.Contains(testCategoryDomainModel, testDomainModel.Categories);
            Assert.Contains(testWalletDomainModel, testDomainModel.Wallets);
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _goalService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task HardDeleteAsync_WhenGoalNotFound_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.HardDeleteAsync(id)).ReturnsAsync((Goal?)null);

            // Act
            var result = await _goalService.HardDeleteAsync(id);

            // Assert
            Assert.False(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenGoalExists_ShouldReturnTrueAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestGoalModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.HardDeleteAsync(id)).ReturnsAsync(testDomainModel);

            // Act
            var result = await _goalService.HardDeleteAsync(id);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _goalRepositoryMock.Reset();
            _categoryRepositoryMock.Reset();
            _walletRepositoryMock.Reset();
        }
    }
}
