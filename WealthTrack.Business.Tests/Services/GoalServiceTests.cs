using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
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
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly IGoalService _goalService;

        public GoalServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _goalRepositoryMock = new Mock<IGoalRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.GoalRepository).Returns(_goalRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.CategoryRepository).Returns(_categoryRepositoryMock.Object);
            _goalService = new GoalService(_unitOfWorkMock.Object, _mapperMock.Object, _eventPublisherMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ModelIsNotNull_ShouldCreateGoalSuccessfully()
        {
            // Arrange
            var testDomainModel = TestGoalModels.DomainModel;
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var testCategoryDomainModel = TestCategoryModels.DomainModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Goal>(testUpsertBusinessModel)).Returns(testDomainModel);
            _goalRepositoryMock.Setup(r => r.CreateAsync(It.Is<Goal>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(testCategoryDomainModel);

            // Act
            var result = await _goalService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            Assert.Contains(testCategoryDomainModel, testDomainModel.Categories);
            _mapperMock.Verify(m => m.Map<Goal>(testUpsertBusinessModel), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<GoalCreatedEvent>()), Times.Once);
            _goalRepositoryMock.Verify(r => r.CreateAsync(It.Is<Goal>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
            Assert.InRange(testDomainModel.CreatedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            Assert.InRange(testDomainModel.ModifiedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            GoalUpsertBusinessModel? model = null;

            // Act
            var act = async () => await _goalService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Goal>(It.IsAny<GoalUpsertBusinessModel>()), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<GoalCreatedEvent>()), Times.Never);
            _goalRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Goal>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var act = async () => await _goalService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<GoalUpsertBusinessModel>(It.IsAny<Goal>()), Times.Never);
            _goalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenGoalNotFound_ShouldReturnNull()
        {
            // Arrange
            var goalId = Guid.NewGuid();
            _goalRepositoryMock.Setup(repo => repo.GetByIdAsync(goalId, It.IsAny<string>())).ReturnsAsync((Goal?)null);

            // Act
            var result = await _goalService.GetByIdAsync(goalId);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<GoalDetailsBusinessModel>(null), Times.Once);
            _goalRepositoryMock.Verify(r => r.GetByIdAsync(goalId, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenGoalFound_ShouldReturnMappedGoal()
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
            _mapperMock.Verify(m => m.Map<GoalDetailsBusinessModel>(testDomainModel), Times.Once);
            _goalRepositoryMock.Verify(r => r.GetByIdAsync(goalId, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoGoalsFound_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyGoalsList = new List<Goal>();
            _goalRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyGoalsList);
            _mapperMock.Setup(m => m.Map<List<GoalDetailsBusinessModel>>(emptyGoalsList)).Returns(new List<GoalDetailsBusinessModel>());

            // Act
            var result = await _goalService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _mapperMock.Verify(m => m.Map<List<GoalDetailsBusinessModel>>(emptyGoalsList), Times.Once);
            _goalRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenGoalsFound_ShouldReturnMappedGoals()
        {
            // Arrange
            var goals = new List<Goal> { TestGoalModels.DomainModel };
            var expectedBusinessModels = new List<GoalDetailsBusinessModel>
            {
                TestGoalModels.DetailsBusinessModel
            };
            var expectedSize = expectedBusinessModels.Count;

            _goalRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(goals);
            _mapperMock.Setup(m => m.Map<List<GoalDetailsBusinessModel>>(goals)).Returns(expectedBusinessModels);

            // Act
            var result = await _goalService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedSize);
            result.Should().BeEquivalentTo(expectedBusinessModels);
            _mapperMock.Verify(m => m.Map<List<GoalDetailsBusinessModel>>(goals), Times.Once);
            _goalRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            var act = async () => await _goalService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Goal>()), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<GoalUpdatedEvent>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.Update(It.IsAny<Goal>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenGoalNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Goal?)null);

            // Act
            var act = async () => await _goalService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, It.IsAny<Goal>()), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<GoalUpdatedEvent>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.Update(It.IsAny<Goal>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenGoalFound_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestGoalModels.UpsertBusinessModel;
            var testDomainModel = TestGoalModels.DomainModel;
            var testCategoryDomainModel = TestCategoryModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(testCategoryDomainModel);

            // Act
            await _goalService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            Assert.Contains(testCategoryDomainModel, testDomainModel.Categories);
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.Update(testDomainModel), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<GoalUpdatedEvent>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            Assert.InRange(testDomainModel.ModifiedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var act = async () => await _goalService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.HardDelete(It.IsAny<Goal>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenGoalNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Goal?)null);

            // Act
            var act = async () => await _goalService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.HardDelete(It.IsAny<Goal>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenGoalFound_ShouldDeleteGoalAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestGoalModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
           await _goalService.HardDeleteAsync(id);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.GetByIdAsync(id, It.IsAny<string>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.GoalRepository.HardDelete(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _goalRepositoryMock.Reset();
            _categoryRepositoryMock.Reset();
        }
    }
}
