using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Application.CQRS.Queries.Currency
{
    public class GetCurrencyRateQuery : IRequest<decimal>
    {
        private readonly GetCurrencyRateQueryValidator _validator = new();

        public GetCurrencyRateQuery(string fromCurrency, string toCurrency)
        {
             FromCurrency = fromCurrency;
            ToCurrency = toCurrency;

            _validator.ValidateAndThrow(this);
        }

        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
    }

    public class GetCurrencyRateQueryValidator : AbstractValidator<GetCurrencyRateQuery>
    {
        public GetCurrencyRateQueryValidator()
        {
            RuleFor(x => x.FromCurrency).NotEmpty().NotNull();
            RuleFor(x => x.ToCurrency).NotEmpty().NotNull();
        }
    }

    public class GetCurrencyRateQueryHandler : IRequestHandler<GetCurrencyRateQuery, decimal>
    {
        private readonly IMemoryCache _memoryCache;

        public GetCurrencyRateQueryHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<decimal> Handle(GetCurrencyRateQuery query, CancellationToken cancellationToken)
        {
            var cacheKey = $"{query.FromCurrency}-{query.ToCurrency}";
            if (_memoryCache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                return await Task.FromResult(cachedRate);
            }

            // Fake logic
            var rate = 27.50m;
            _memoryCache.Set(cacheKey, rate, TimeSpan.FromMinutes(5));

            return await Task.FromResult(rate);
        }
    }
}
