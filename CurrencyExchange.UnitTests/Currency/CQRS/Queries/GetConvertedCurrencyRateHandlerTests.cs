using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.CQRS.Queries.Currency;
using CurrencyExchange.Application.Interfaces;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CurrencyExchange.UnitTests.Currency.CQRS.Queries
{
    public class GetConvertedCurrencyRateHandlerTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ICurrencyProvider> _providerMock;
        private readonly Mock<ICurrencyValidator> _currencyValidatorMock;
        private readonly GetConvertedCurrencyRateValidator _validator;
        private readonly FrankFurtherCurrencyProvider _frankFurterProvider;

        public GetConvertedCurrencyRateHandlerTests()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.frankfurter.app") };

            _memoryCacheMock = new Mock<IMemoryCache>();
            _providerMock = new Mock<ICurrencyProvider>();
            _frankFurterProvider = new FrankFurtherCurrencyProvider(httpClient);
            _currencyValidatorMock = new Mock<ICurrencyValidator>();
            _currencyValidatorMock.Setup(v => v.ValidCurrencies).Returns(new HashSet<string> { "USD", "EUR" });
            _currencyValidatorMock.Setup(v => v.ForbiddenCurrencies).Returns(new HashSet<string> { "TRY" });

            _validator = new GetConvertedCurrencyRateValidator(_currencyValidatorMock.Object);
        }
        [Fact]
        public void Should_HaveValidationError_When_CurrenciesAreUnsupported()
        {
            var query = new GetConvertedCurrencyRate("ABC", "XYZ");
            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.FromCurrency)
                  .WithErrorMessage("Invalid or unsupported FromCurrency.");

            result.ShouldHaveValidationErrorFor(x => x.ToCurrency)
                  .WithErrorMessage("Invalid or unsupported ToCurrency.");
        }

        [Fact]
        public async Task Handle_ShouldReturnRateFromCache_IfExists()
        {
            var query = new GetConvertedCurrencyRate("USD", "EUR");
            var expectedRate = 1.23m;

            object cacheValue = expectedRate;
            _memoryCacheMock.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(true);

            var handler = new GetConvertedCurrencyRateHandler(_memoryCacheMock.Object, _frankFurterProvider, _currencyValidatorMock.Object);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ConvertedRate.Should().Be(expectedRate);
        }

        [Fact]
        public async Task Handle_ShouldCallProvider_IfNotCached()
        {
            var query = new GetConvertedCurrencyRate("USD", "EUR");

            object dummy = null;
            _memoryCacheMock.Setup(mc => mc.TryGetValue(It.IsAny<object>(), out dummy)).Returns(false);

            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>);


            var handler = new GetConvertedCurrencyRateHandler(
                _memoryCacheMock.Object,
                _frankFurterProvider,
                _currencyValidatorMock.Object);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ConvertedRate.Should().BeGreaterThan(0);
        }
    }
}
