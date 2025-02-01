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
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenIdIsEmpty()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act
            Func<Task> act = async () => await _currencyService.GetByIdAsync(emptyId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCurrencyNotFound()
        {
            // Arrange
            var currencyId = Guid.NewGuid();
            _currencyRepositoryMock.Setup(repo => repo.GetByIdAsync(currencyId, It.IsAny<string>())).ReturnsAsync((Currency?)null);

            // Act
            var result = await _currencyService.GetByIdAsync(currencyId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedCurrency_WhenCurrencyExists()
        {
            // Arrange
            var testDetailsBusinessModel = TestCurrencyModels.DetailsBusinessModel;
            var testDomainModel = TestCurrencyModels.DomainModel;
            var currencyId = testDomainModel.Id;
            _currencyRepositoryMock.Setup(repo => repo.GetByIdAsync(currencyId, It.IsAny<string>())).ReturnsAsync(testDomainModel);
            _mapperMock.Setup(m => m.Map<CurrencyDetailsBusinessModel>(testDomainModel)).Returns(testDetailsBusinessModel);

            // Act
            var result = await _currencyService.GetByIdAsync(currencyId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(testDetailsBusinessModel);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCurrenciesExist()
        {
            // Arrange
            var emptyCurrenciesList = new List<Currency>();
            _currencyRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(emptyCurrenciesList);
            _mapperMock.Setup(m => m.Map<List<CurrencyDetailsBusinessModel>>(emptyCurrenciesList)).Returns(new List<CurrencyDetailsBusinessModel>());

            // Act
            var result = await _currencyService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedCurrencies_WhenCurrenciesExist()
        {
            // Arrange
            var currencies = new List<Currency> { TestCurrencyModels.DomainModel };
            var expectedBusinessModels = new List<CurrencyDetailsBusinessModel>
            {
                TestCurrencyModels.DetailsBusinessModel
            };

            _currencyRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<string>())).ReturnsAsync(currencies);
            _mapperMock.Setup(m => m.Map<List<CurrencyDetailsBusinessModel>>(currencies)).Returns(expectedBusinessModels);

            // Act
            var result = await _currencyService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeEquivalentTo(expectedBusinessModels);
        }

        public void Dispose()
        {
            _mapperMock.Reset();
            _unitOfWorkMock.Reset();
            _currencyRepositoryMock.Reset();
        }
    }
}
