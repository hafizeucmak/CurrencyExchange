using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.DTOs.Currency;
using CurrencyExchange.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Application.CQRS.Queries.Currency
{
    public class GetConvertedCurrencyRate : IRequest<CurrencyConversionOutputDto>
    {
        public GetConvertedCurrencyRate(string fromCurrency, string toCurrency)
        {
            FromCurrency = fromCurrency;
            ToCurrency = toCurrency;
        }

        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
    }

    public class GetConvertedCurrencyRateValidator : AbstractValidator<GetConvertedCurrencyRate>
    {
        public GetConvertedCurrencyRateValidator(ICurrencyValidator validator)
        {
            RuleFor(x => x.FromCurrency).NotEmpty().WithMessage("FromCurrency is required.")
                                        .Must(currency => validator.ValidCurrencies.Contains(currency))
                                        .WithMessage("Invalid or unsupported FromCurrency.")
                                        .Must(currency => !validator.ForbiddenCurrencies.Contains(currency))
                                        .WithMessage("FromCurrency is not allowed for conversion.");
            RuleFor(x => x.ToCurrency).NotEmpty()
                                      .NotNull().Must(currency => validator.ValidCurrencies.Contains(currency))
                                      .WithMessage("Invalid or unsupported ToCurrency.")
                                      .Must(currency => !validator.ForbiddenCurrencies.Contains(currency))
                                      .WithMessage("ToCurrency is not allowed for conversion.");
        }
    }

    public class GetConvertedCurrencyRateHandler : IRequestHandler<GetConvertedCurrencyRate, CurrencyConversionOutputDto>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly FrankFurtherCurrencyProvider _provider;
        private readonly ICurrencyValidator _currencyValidator;

        public GetConvertedCurrencyRateHandler(IMemoryCache memoryCache, FrankFurtherCurrencyProvider provider, ICurrencyValidator currencyValidator)
        {
            _memoryCache = memoryCache;
            _provider = provider;
            _currencyValidator = currencyValidator;
        }

        public async Task<CurrencyConversionOutputDto> Handle(GetConvertedCurrencyRate query, CancellationToken cancellationToken)
        {
            var validator = new GetConvertedCurrencyRateValidator(_currencyValidator);
            validator.ValidateAndThrow(query);

            var cacheKey = $"{query.FromCurrency}-{query.ToCurrency}";
            if (_memoryCache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                return await Task.FromResult(new CurrencyConversionOutputDto() { ConvertedRate = cachedRate });
            }

            var rateResult = await _provider.GetExchangeRateAsync(query.FromCurrency, query.ToCurrency);

            _memoryCache.Set(cacheKey, rateResult, TimeSpan.FromMinutes(5));

            return await Task.FromResult(new CurrencyConversionOutputDto() { ConvertedRate = rateResult });
        }
    }
}
