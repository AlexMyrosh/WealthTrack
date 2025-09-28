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
        private readonly Mock<ITransferTransactionRepository> _transferTransactionRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly ITransactionService _transactionService;

        public TransactionServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _transferTransactionRepositoryMock = new Mock<ITransferTransactionRepository>();
            _mapperMock = new Mock<IMapper>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            _unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TransferTransactionRepository).Returns(_transferTransactionRepositoryMock.Object);
            _transactionService = new TransactionService(_unitOfWorkMock.Object, _mapperMock.Object, _eventPublisherMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNotNull_ShouldCreateTransactionSuccessfully()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var testTransactionCreatedEvent = new TransactionCreatedEvent();
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<Transaction>(testUpsertBusinessModel)).Returns(testDomainModel);
            _mapperMock.Setup(m => m.Map<TransactionCreatedEvent>(testDomainModel)).Returns(testTransactionCreatedEvent);
            _transactionRepositoryMock.Setup(r => r.CreateAsync(It.Is<Transaction>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _transactionService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            _mapperMock.Verify(m => m.Map<Transaction>(testUpsertBusinessModel), Times.Once);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.Is<Transaction>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionCreatedEvent>()), Times.Once);
            Assert.InRange(testDomainModel.CreatedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
        }

        [Fact]
        public async Task CreateAsync_WhenTransactionIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            TransactionUpsertBusinessModel? model = null;

            // Act
            var act = async () => await _transactionService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionCreatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenModelIsNotNull_ShouldCreateTransferTransactionSuccessfully()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransferTransactionDomainModel;
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var testTransferTransactionCreatedEvent = new TransferTransactionCreatedEvent();
            var expectedId = testDomainModel.Id;
            _mapperMock.Setup(m => m.Map<TransferTransaction>(testUpsertBusinessModel)).Returns(testDomainModel);
            _mapperMock.Setup(m => m.Map<TransferTransactionCreatedEvent>(testDomainModel)).Returns(testTransferTransactionCreatedEvent);
            _transferTransactionRepositoryMock.Setup(r => r.CreateAsync(It.Is<TransferTransaction>(x => x.Equals(testDomainModel)))).ReturnsAsync(expectedId);

            // Act
            var result = await _transactionService.CreateAsync(testUpsertBusinessModel);

            // Assert
            result.Should().Be(expectedId);
            // TODO: Add check for related data
            Assert.InRange(testDomainModel.CreatedDate, DateTimeOffset.Now.AddMinutes(-1), DateTimeOffset.Now);
            _mapperMock.Verify(m => m.Map<TransferTransaction>(testUpsertBusinessModel), Times.Once);
            _transferTransactionRepositoryMock.Verify(r => r.CreateAsync(It.Is<TransferTransaction>(b => b.Equals(testDomainModel))), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(testTransferTransactionCreatedEvent), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenTransferTransactionIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            TransferTransactionUpsertBusinessModel? model = null;

            // Act
            var act = async () => await _transactionService.CreateAsync(model!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionCreatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var act = async () => await _transactionService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<TransactionDetailsBusinessModel>(It.IsAny<Transaction>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTransactionNotFound_ShouldReturnNull()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            _transactionRepositoryMock.Setup(repo => repo.GetByIdAsync(transactionId, It.IsAny<string>())).ReturnsAsync((Transaction?)null);

            // Act
            var result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<TransactionDetailsBusinessModel>(It.IsAny<Transaction>()), Times.Once);
            _transactionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTransactionFound_ShouldReturnMappedTransaction()
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
            _mapperMock.Verify(m => m.Map<TransactionDetailsBusinessModel>(It.IsAny<Transaction>()), Times.Once);
            _transactionRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTransactionsFound_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyTransactionsList = new List<Transaction>();
            _transactionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyTransactionsList);
            _mapperMock.Setup(m => m.Map<List<TransactionDetailsBusinessModel>>(emptyTransactionsList)).Returns(new List<TransactionDetailsBusinessModel>());

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _mapperMock.Verify(m => m.Map<List<TransactionDetailsBusinessModel>>(emptyTransactionsList), Times.Once);
            _transactionRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenTransactionsFound_ShouldReturnMappedTransactions()
        {
            // Arrange
            var transactions = new List<Transaction> { TestTransactionModels.TransactionDomainModel };
            var expectedBusinessModels = new List<TransactionDetailsBusinessModel>
            {
                TestTransactionModels.DetailsBusinessModel
            };
            var expectedSize = expectedBusinessModels.Count;

            _transactionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(transactions);
            _mapperMock.Setup(m => m.Map<List<TransactionDetailsBusinessModel>>(transactions)).Returns(expectedBusinessModels);

            // Act
            var result = await _transactionService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedSize);
            result.Should().BeEquivalentTo(expectedBusinessModels);
            _mapperMock.Verify(m => m.Map<List<TransactionDetailsBusinessModel>>(transactions), Times.Once);
            _transactionRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TransactionModel_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var id = Guid.Empty;

            // Act
            var act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
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
            var act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.Update(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionUpdatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransactionFound_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransactionBusinessModel;
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.TransactionRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TransferTransactionModel_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var id = Guid.Empty;

            // Act
            var act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _mapperMock.Verify(m => m.Map<Transaction>(It.IsAny<TransferTransactionUpsertBusinessModel>()), Times.Never);
            _transactionRepositoryMock.Verify(r => r.Update(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionUpdatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransferTransactionNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.TransferTransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((TransferTransaction?)null);

            // Act
            var act = async () => await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _mapperMock.Verify(m => m.Map<TransferTransaction>(It.IsAny<TransferTransactionUpsertBusinessModel>()), Times.Never);
            _transferTransactionRepositoryMock.Verify(r => r.Update(It.IsAny<TransferTransaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionUpdatedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTransferTransactionFound_ShouldUpdateSuccessfully()
        {
            // Arrange
            var testUpsertBusinessModel = TestTransactionModels.UpsertTransferTransactionBusinessModel;
            var testDomainModel = TestTransactionModels.TransferTransactionDomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransferTransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _transactionService.UpdateAsync(id, testUpsertBusinessModel);

            // Assert
            _mapperMock.Verify(m => m.Map(testUpsertBusinessModel, testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.TransferTransactionRepository.Update(testDomainModel), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var act = async () => await _transactionService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _transactionRepositoryMock.Verify(r => r.HardDelete(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenTransactionNotFound_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync((Transaction?)null);

            // Act
            var act = async () => await _transactionService.HardDeleteAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _transactionRepositoryMock.Verify(r => r.HardDelete(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Never);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionDeletedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenTransactionFound_ShouldDeleteBudgetAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransactionDomainModel;
            testDomainModel.Type = OperationType.Income;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _transactionService.HardDeleteAsync(id);

            // Assert
            _transactionRepositoryMock.Verify(u => u.HardDelete(It.IsAny<Transaction>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransactionDeletedEvent>()), Times.Once);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenTransferTransactionFound_ShouldDeleteBudgetAndSaveChanges()
        {
            // Arrange
            var testDomainModel = TestTransactionModels.TransferTransactionDomainModel;
            var id = testDomainModel.Id;
            _unitOfWorkMock.Setup(uow => uow.TransferTransactionRepository.GetByIdAsync(id, It.IsAny<string>())).ReturnsAsync(testDomainModel);

            // Act
            await _transactionService.HardDeleteAsync(id);

            // Assert
            _transferTransactionRepositoryMock.Verify(u => u.HardDelete(It.IsAny<TransferTransaction>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
            _eventPublisherMock.Verify(u => u.PublishAsync(It.IsAny<TransferTransactionDeletedEvent>()), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _transactionRepositoryMock.Reset();
            _transferTransactionRepositoryMock.Reset();
            _eventPublisherMock.Reset();
        }
    }
}
