using AutoMapper;
using Moq;
using FluentAssertions;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;

public class BudgetServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly BudgetService _budgetService;
    private readonly Budget _testDomainModel;
    private readonly BudgetUpsertBusinessModel _testUpsertBusinessModel;
    private readonly BudgetDetailsBusinessModel _testDetailsBusinessModel;

    public BudgetServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _budgetRepositoryMock = new Mock<IBudgetRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.BudgetRepository).Returns(_budgetRepositoryMock.Object);
        _budgetService = new BudgetService(_unitOfWorkMock.Object, _mapperMock.Object);

        _testDomainModel = new Budget
        {
            Id = Guid.NewGuid(),
            Name = "Test Budget",
            CurrencyId = new Guid()
        };

        _testUpsertBusinessModel = new BudgetUpsertBusinessModel()
        {
            Name = _testDomainModel.Name,
            CurrencyId = _testDomainModel.CurrencyId
        };

        _testDetailsBusinessModel = new BudgetDetailsBusinessModel
        {
            Id = _testDomainModel.Id,
            Name = _testDomainModel.Name,
            OverallBalance = _testDomainModel.OverallBalance
        };
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBudgetSuccessfully()
    {
        // Arrange
        var expectedId = _testDomainModel.Id;
        _mapperMock.Setup(m => m.Map<Budget>(_testUpsertBusinessModel)).Returns(_testDomainModel);
        _budgetRepositoryMock.Setup(r => r.CreateAsync(It.Is<Budget>(x=> x.Equals(_testDomainModel)))).ReturnsAsync(expectedId);

        // Act
        var result = await _budgetService.CreateAsync(_testUpsertBusinessModel);

        // Assert
        result.Should().Be(expectedId);
        _mapperMock.Verify(m => m.Map<Budget>(_testUpsertBusinessModel), Times.Once);
        _budgetRepositoryMock.Verify(r => r.CreateAsync(It.Is<Budget>(b => b.Equals(_testDomainModel))), Times.Once);
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
        var budgetId = _testDomainModel.Id;
        _budgetRepositoryMock.Setup(repo => repo.GetByIdAsync(budgetId, "")).ReturnsAsync(_testDomainModel);
        _mapperMock.Setup(m => m.Map<BudgetDetailsBusinessModel>(_testDomainModel)).Returns(_testDetailsBusinessModel);

        // Act
        var result = await _budgetService.GetByIdAsync(budgetId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_testDetailsBusinessModel);
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
        var budgets = new List<Budget> { _testDomainModel };
        var expectedBusinessModels = new List<BudgetDetailsBusinessModel>
        {
            _testDetailsBusinessModel
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
        var id = Guid.Empty;

        // Act
        Func<Task> act = async () => await _budgetService.UpdateAsync(id, _testUpsertBusinessModel);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenBudgetNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, "")).ReturnsAsync((Budget?)null);

        // Act
        Func<Task> act = async () => await _budgetService.UpdateAsync(id, _testUpsertBusinessModel);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenBudgetExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(uow => uow.BudgetRepository.GetByIdAsync(id, "")).ReturnsAsync(_testDomainModel);
        _mapperMock.Setup(m => m.Map(_testUpsertBusinessModel, _testDomainModel));

        // Act
        await _budgetService.UpdateAsync(id, _testUpsertBusinessModel);

        // Assert
        _mapperMock.Verify(m => m.Map(_testUpsertBusinessModel, _testDomainModel), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.BudgetRepository.Update(_testDomainModel), Times.Once);
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
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(uow => uow.BudgetRepository.HardDeleteAsync(id)).ReturnsAsync(_testDomainModel);

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
