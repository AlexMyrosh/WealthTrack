using AutoMapper;
using FluentAssertions;
using Moq;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Business.Tests.TestModels;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.Tests.Services
{
    public class CurrencyServiceTests : IDisposable
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ICurrencyService _currencyService;

        public CurrencyServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currencyRepositoryMock = new Mock<ICurrencyRepository>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.CurrencyRepository).Returns(_currencyRepositoryMock.Object);
            _currencyService = new CurrencyService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            var act = async () => await _currencyService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            _currencyRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCurrencyNotFound_ShouldReturnNull()
        {
            // Arrange
            var currencyId = Guid.NewGuid();
            _currencyRepositoryMock.Setup(repo => repo.GetByIdAsync(currencyId)).ReturnsAsync((Currency?)null);

            // Act
            var result = await _currencyService.GetByIdAsync(currencyId);

            // Assert
            result.Should().BeNull();
            _currencyRepositoryMock.Verify(r=>r.GetByIdAsync(currencyId), Times.Once);
            _mapperMock.Verify(m=>m.Map<CurrencyDetailsBusinessModel>(null), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCurrencyFound_ShouldReturnMappedCurrency()
        {
            // Arrange
            var testDetailsBusinessModel = TestCurrencyModels.DetailsBusinessModel;
            var testDomainModel = TestCurrencyModels.DomainModel;
            var currencyId = testDomainModel.Id;
            _currencyRepositoryMock.Setup(repo => repo.GetByIdAsync(currencyId)).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<CurrencyDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _currencyService.GetByIdAsync(currencyId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
            _currencyRepositoryMock.Verify(r => r.GetByIdAsync(currencyId), Times.Once);
            _mapperMock.Verify(m => m.Map<CurrencyDetailsBusinessModel>(testDomainModel), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoCurrenciesFound_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyCurrenciesList = new List<Currency>();
            _currencyRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(emptyCurrenciesList);
            _mapperMock.Setup(m => m.Map<List<CurrencyDetailsBusinessModel>>(emptyCurrenciesList)).Returns(new List<CurrencyDetailsBusinessModel>());

            // Act
            var result = await _currencyService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _currencyRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<CurrencyDetailsBusinessModel>>(emptyCurrenciesList), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenCurrenciesFound_ShouldReturnMappedCurrencies()
        {
            // Arrange
            var currencies = new List<Currency> { TestCurrencyModels.DomainModel };
            var expectedBusinessModels = new List<CurrencyDetailsBusinessModel>
            {
                TestCurrencyModels.DetailsBusinessModel
            };
            var expectedSize = expectedBusinessModels.Count;

            _currencyRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(currencies);
            _mapperMock.Setup(m => m.Map<List<CurrencyDetailsBusinessModel>>(currencies)).Returns(expectedBusinessModels);

            // Act
            var result = await _currencyService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedSize);
            result.Should().BeEquivalentTo(expectedBusinessModels);
            _mapperMock.Verify(m => m.Map<List<CurrencyDetailsBusinessModel>>(currencies), Times.Once);
            _currencyRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _currencyRepositoryMock.Reset();
        }
    }
}
