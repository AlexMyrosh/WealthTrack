using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WealthTrack.Business.BusinessModels.Transaction;
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
    public class TransactionServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly ITransactionService _transactionService;
        private readonly Guid _testTransferCategoryId;

        public TransactionServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _testTransferCategoryId = Guid.NewGuid();
            _configurationMock.Setup(m => m["SystemCategories:TransferId"]).Returns(_testTransferCategoryId.ToString());

            _unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepositoryMock.Object);
            _transactionService = new TransactionService(_unitOfWorkMock.Object, _mapperMock.Object, _eventPublisherMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Transaction_ShouldCreateSuccessfully()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Transaction>(testUpsertBusinessModel)).Returns(testDomainModel);
            _transactionRepositoryMock.Setup(r => r.CreateAsync(It.Is<Transaction>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _transactionService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            _mapperMock.Verify(m => m.Map<Transaction>(testUpsertBusinessModel), Times.Once);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.Is<Transaction>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionAddedEvent>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Transaction_ShouldThrowException_WhenModelIsNull()
        {
            // Arrange
            TransactionUpsertBusinessModel? model = null;

            // Act
            Func<Task> act = async () => await _transactionService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionAddedEvent>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_TransactionTransfer_ShouldCreateSuccessfully()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransferTransactionDomainModel;
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Transaction>(testUpsertBusinessModel)).Returns(testDomainModel);
            _transactionRepositoryMock.Setup(r => r.CreateAsync(It.Is<Transaction>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _transactionService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            Assert.Equal(testDomainModel.Type, TransactionType.Transfer);
            Assert.Equal(testDomainModel.CategoryId, _testTransferCategoryId);
            _mapperMock.Verify(m => m.Map<Transaction>(testUpsertBusinessModel), Times.Once);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.Is<Transaction>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_TransactionTransfer_ShouldThrowException_WhenModelIsNull()
        {
            // Arrange
            TransferTransactionUpsertBusinessModel? model = null;

            // Act
            Func<Task> act = async () => await _transactionService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenIdIsEmpty()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            Func<Task> act = async () => await _transactionService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionNotFound()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            _transactionRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionId, It.IsAny<string>())).ReturnsAsync((Transaction?)null);

            // Act
            var result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedTransaction_WhenTransactionExists()
        {
            // Arrange
            var testDetailsBusinessModel = TestTransactionModels.DetailsBusinessModel;
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var transactionId = testDomainModel.Id;
            _transactionRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionId, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<TransactionDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoTransactionsExist()
        {
            // Arrange
            var emptyTransactionsList = new List<Transaction>();
            _transactionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyTransactionsList);
            _mapperMock.Setup(m => m.Map<List<TransactionDetailsBusinessModel>>(emptyTransactionsList)).Returns(new List<TransactionDetailsBusinessModel>());

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedTransactions_WhenTransactionsExist()
        {
            // Arrange
            var transactions = new List<Transaction> { TestTransactionModels.TransactionDomainModel };
            var expectedBusinessModels = new List<TransactionDetailsBusinessModel>
            {
                TestTransactionModels.DetailsBusinessModel
            };

            _transactionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(transactions);
            _mapperMock.Setup(m => m.Map<List<TransactionDetailsBusinessModel>>(transactions)).Returns(expectedBusinessModels);

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeEquivalentTo(expectedBusinessModels);
        }

        [Fact]
        public async Task UpdateAsync_TransactionModel_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.Update(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionUpdatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransactionNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Transaction?)null);

            // Act
            Func<Task> act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.Update(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionUpdatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransactionExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map(testUpsertBusinessModel, testDomainModel));

            // Act
            await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.TransactionRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TransactionTransferModel_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransferTransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.Update(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransferTransactionNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Transaction?)null);

            // Act
            Func<Task> act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransferTransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.Update(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransferTransactionExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map(testUpsertBusinessModel, testDomainModel));

            // Act
            await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.TransactionRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            Func<Task> act = async () => await _transactionService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _transactionRepositoryMock.Verify(r => r.HardDelete(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenTransactionNotFound_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Transaction?)null);

            // Act
            Func<Task> act = async () => await _transactionService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _transactionRepositoryMock.Verify(r => r.HardDelete(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenTransactionExists_ShouldReturnTrueAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            var result = await _transactionService.HardDeleteAsync(id);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Once);
            _transactionRepositoryMock.Verify(u => u.HardDelete(It.IsAny<Transaction>()), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _transactionRepositoryMock.Reset();
            _configurationMock.Reset();
            _eventPublisherMock.Reset();
        }
    }
}
