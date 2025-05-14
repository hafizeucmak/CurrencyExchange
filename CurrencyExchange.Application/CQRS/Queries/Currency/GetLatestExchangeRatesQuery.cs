using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.DTOs.Currency;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Application.CQRS.Queries.Currency
{
    public class GetLatestExchangeRatesQuery : IRequest<LatestExchangeRatesOutputDto>
    {
        private readonly GetLatestExchangeRatesQueryValidator _validator = new();

        public GetLatestExchangeRatesQuery(string baseCurrency)
        {
            BaseCurrency = baseCurrency;

            _validator.ValidateAndThrow(this);
        }

        public string BaseCurrency { get; set; }
    }

    public class GetLatestExchangeRatesQueryValidator : AbstractValidator<GetLatestExchangeRatesQuery>
    {
        public GetLatestExchangeRatesQueryValidator()
        {
            RuleFor(x => x.BaseCurrency).NotEmpty().NotNull();
        }
    }

    public class GetLatestExchangeRatesQueryHandler : IRequestHandler<GetLatestExchangeRatesQuery, LatestExchangeRatesOutputDto>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly FrankFurtherCurrencyProvider _provider;

        public GetLatestExchangeRatesQueryHandler(IMemoryCache memoryCache, FrankFurtherCurrencyProvider provider)
        {
            _memoryCache = memoryCache;
            _provider = provider;
        }

        public async Task<LatestExchangeRatesOutputDto> Handle(GetLatestExchangeRatesQuery query, CancellationToken cancellationToken)
        {
            string cacheKey = $"ExchangeRates:{query.BaseCurrency.ToUpper()}";

            if (_memoryCache.TryGetValue(cacheKey, out var cachedResult) && cachedResult is LatestExchangeRatesOutputDto result)
            {
                return result;
            }

            var providerResult = await _provider.GetLatestRatesAsync(query.BaseCurrency);

            var output = new LatestExchangeRatesOutputDto
            {
                BaseCurrency = providerResult.BaseCurrency,
                Date = providerResult.Date,
                Rates = providerResult.Rates
            };

            _memoryCache.Set(cacheKey, output, TimeSpan.FromMinutes(10));

            return output;
        }
    }
}
