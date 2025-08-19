using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WealthTrack.Business.BusinessModels.Wallet;
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
    public class WalletServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly IWalletService _walletService;
        private readonly Guid _testBalanceCorrectionCategoryId;

        public WalletServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _testBalanceCorrectionCategoryId = Guid.NewGuid();
            _configurationMock.Setup(m => m["SystemCategories:BalanceCorrectionId"]).Returns(_testBalanceCorrectionCategoryId.ToString());

            _unitOfWorkMock.Setup(u => u.WalletRepository).Returns(_walletRepositoryMock.Object);
            _walletService = new WalletService(_unitOfWorkMock.Object, _mapperMock.Object, _eventPublisherMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNotNull_ShouldCreateWalletSuccessfully()
        {
            // Arrange
            var testDomainModel = TestWalletModels.DomainModel;
            var testUpsertBusinessModel = TestWalletModels.UpsertBusinessModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Wallet>(testUpsertBusinessModel)).Returns(testDomainModel);
            _walletRepositoryMock.Setup(r => r.CreateAsync(It.Is<Wallet>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _walletService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            _mapperMock.Verify(m => m.Map<Wallet>(testUpsertBusinessModel), Times.Once);
            _walletRepositoryMock.Verify(r => r.CreateAsync(It.Is<Wallet>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletCreatedEvent>()), Times.Once);
            Assert.InRange(testDomainModel.CreatedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            Assert.InRange(testDomainModel.ModifiedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNull_ShouldThrowException()
        {
            // Arrange
            WalletUpsertBusinessModel? model = null;

            // Act
            var act = async () => await _walletService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Wallet>(It.IsAny<WalletUpsertBusinessModel>()), Times.Never);
            _walletRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Wallet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletCreatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var act = async () => await _walletService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<WalletDetailsBusinessModel>(It.IsAny<Wallet>()), Times.Never);
            _walletRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenWalletNotFound_ShouldReturnNull()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            _walletRepositoryMock.Setup(repo => repo.GetByIdAsync(walletId, It.IsAny<string>())).ReturnsAsync((Wallet?)null);

            // Act
            var result = await _walletService.GetByIdAsync(walletId);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<WalletDetailsBusinessModel>(It.IsAny<Wallet>()), Times.Once);
            _walletRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenWalletFound_ShouldReturnMappedWallet()
        {
            // Arrange
            var testDetailsBusinessModel = TestWalletModels.DetailsBusinessModel;
            var testDomainModel = TestWalletModels.DomainModel;
            var walletId = testDomainModel.Id;
            _walletRepositoryMock.Setup(repo => repo.GetByIdAsync(walletId, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<WalletDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _walletService.GetByIdAsync(walletId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
            _mapperMock.Verify(m => m.Map<WalletDetailsBusinessModel>(It.IsAny<Wallet>()), Times.Once);
            _walletRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoWalletsFound_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyWalletsList = new List<Wallet>();
            _walletRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyWalletsList);
            _mapperMock.Setup(m => m.Map<List<WalletDetailsBusinessModel>>(emptyWalletsList)).Returns(new List<WalletDetailsBusinessModel>());

            // Act
            var result = await _walletService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _mapperMock.Verify(m => m.Map<List<WalletDetailsBusinessModel>>(emptyWalletsList), Times.Once);
            _walletRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenWalletsFound_ShouldReturnMappedWallets()
        {
            // Arrange
            var wallets = new List<Wallet> { TestWalletModels.DomainModel };
            var expectedBusinessModels = new List<WalletDetailsBusinessModel>
            {
                TestWalletModels.DetailsBusinessModel
            };
            var expectedSize = expectedBusinessModels.Count;

            _walletRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(wallets);
            _mapperMock.Setup(m => m.Map<List<WalletDetailsBusinessModel>>(wallets)).Returns(expectedBusinessModels);

            // Act
            var result = await _walletService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedSize);
            result.Should().BeEquivalentTo(expectedBusinessModels);
            _mapperMock.Verify(m => m.Map<List<WalletDetailsBusinessModel>>(wallets), Times.Once);
            _walletRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var testUpsertBusinessModel = TestWalletModels.UpsertBusinessModel;
            var id = Guid.Empty;

            // Act
            var act = async () => await _walletService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<Wallet>(It.IsAny<WalletUpsertBusinessModel>()), Times.Never);
            _walletRepositoryMock.Verify(r => r.Update(It.IsAny<Wallet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletUpdatedEvent>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TransactionRepository.CreateAsync(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenWalletNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestWalletModels.UpsertBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.WalletRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Wallet?)null);

            // Act
            var act = async () => await _walletService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map<Wallet>(It.IsAny<WalletUpsertBusinessModel>()), Times.Never);
            _walletRepositoryMock.Verify(r => r.Update(It.IsAny<Wallet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletUpdatedEvent>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TransactionRepository.CreateAsync(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenWalletFound_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestWalletModels.UpsertBusinessModel;
            var testDomainModel = TestWalletModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.WalletRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _walletService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.WalletRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenWalletFoundWithDifferentBalance_ShouldCreateBalanceCorrectionTransaction()
        {
            // Arrange
            var testUpsertBusinessModel = TestWalletModels.UpsertBusinessModel;
            var testDomainModel = TestWalletModels.DomainModel;
            testUpsertBusinessModel.Balance = 100;
            testDomainModel.Balance = 200;
            var correctionAmount = testDomainModel.Balance - testUpsertBusinessModel.Balance;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.WalletRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.CreateAsync(It.IsAny<Transaction>()));

            // Act
            await _walletService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.WalletRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletUpdatedEvent>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.TransactionRepository.CreateAsync(
                    It.Is<Transaction>(t =>
                        t.Amount == correctionAmount &&
                        t.CategoryId == _testBalanceCorrectionCategoryId
                    )),
                Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var act = async () => await _walletService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _walletRepositoryMock.Verify(r => r.HardDeleteAsync(It.IsAny<Wallet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenWalletNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.WalletRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Wallet?)null);

            // Act
            var act = async () => await _walletService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _walletRepositoryMock.Verify(r => r.HardDeleteAsync(It.IsAny<Wallet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenWalletFound_ShouldReturnTrueAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestWalletModels.DomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.WalletRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _walletService.HardDeleteAsync(id);

            // Assert
            _walletRepositoryMock.Verify(r => r.HardDeleteAsync(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<WalletDeletedEvent>()), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _walletRepositoryMock.Reset();
            _configurationMock.Reset();
            _eventPublisherMock.Reset();
        }
    }
}
