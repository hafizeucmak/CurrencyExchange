using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.CQRS.Queries.Currency;
using CurrencyExchange.Application.DTOs.Currency;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CurrencyExchange.UnitTests.Currency.CQRS.Queries
{
    public class GetLatestExchangeRatesQueryHandlerTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<FrankFurtherCurrencyProvider> _frankFurterProvider;
        private readonly FrankFurtherCurrencyProvider _provider;
        private readonly Mock<ICurrencyProvider> _currencyProvider;
        private readonly GetLatestExchangeRatesQueryHandler _handler;

        public GetLatestExchangeRatesQueryHandlerTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();

            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.frankfurter.app") };
            _frankFurterProvider = new Mock<FrankFurtherCurrencyProvider>(MockBehavior.Strict, httpClient);
            _provider = new FrankFurtherCurrencyProvider(httpClient);
            _currencyProvider = new Mock<ICurrencyProvider>();
            _handler = new GetLatestExchangeRatesQueryHandler(_memoryCacheMock.Object,_provider);
        }

        [Fact]
        public async Task Handle_ShouldReturnCachedResult_IfExists()
        {
            var query = new GetLatestExchangeRatesQuery(baseCurrency: "USD");
            var cachedResult = new LatestExchangeRatesOutputDto
            {
                BaseCurrency = "USD",
                Date = DateTime.Today,
                Rates = new Dictionary<string, decimal> { ["EUR"] = 1.1m }
            };

            object cachedObj = cachedResult;
            _memoryCacheMock.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out cachedObj))
                            .Returns(true);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeEquivalentTo(cachedResult);
            _currencyProvider.Verify(p => p.GetLatestRatesAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldCallProvider_AndCacheResult_IfNotCached()
        {
            var query = new GetLatestExchangeRatesQuery("EUR");
            var providerResult = new ExchangeRatesDto
            {
                BaseCurrency = "EUR",
                Date = DateTime.Now,
                Rates = new Dictionary<string, decimal> { { "USD", 1.18m } }
            };
            var cacheKey = "ExchangeRates:EUR";

            object returnedCachedResult = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.Is<string>(s => s == "ExchangeRates:EUR"), out returnedCachedResult))
                .Returns(false);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            _frankFurterProvider.Setup(p => p.GetLatestRatesAsync("EUR")).ReturnsAsync(providerResult);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal(providerResult.BaseCurrency, result.BaseCurrency);

            _memoryCacheMock.Verify(
                m => m.CreateEntry(It.Is<string>(s => s == "ExchangeRates:EUR")),
                Times.Once);
        }
    }
}