using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.DTOs.Currency;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Application.CQRS.Queries.Currency
{
    public class GetHistoricalExchangeRatesQuery : IRequest<PaginatedHistoricalRatesOutputDto>
    {
        private readonly GetHistoricalExchangeRatesQueryValidator _validator = new();

        public GetHistoricalExchangeRatesQuery(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            BaseCurrency = baseCurrency;
            StartDate = startDate;
            EndDate = endDate;
            Page = page;
            PageSize = pageSize;

            _validator.ValidateAndThrow(this);
        }

        public string BaseCurrency { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int Page { get; }
        public int PageSize { get; }
    }

    public class GetHistoricalExchangeRatesQueryValidator : AbstractValidator<GetHistoricalExchangeRatesQuery>
    {
        public GetHistoricalExchangeRatesQueryValidator()
        {
            RuleFor(x => x.BaseCurrency).NotEmpty().WithMessage($"Base currency can not be empty");
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        }
    }

    public class GetHistoricalExchangeRatesQueryHandler : IRequestHandler<GetHistoricalExchangeRatesQuery, PaginatedHistoricalRatesOutputDto>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly FrankFurtherCurrencyProvider _provider;

        public GetHistoricalExchangeRatesQueryHandler(IMemoryCache memoryCache, FrankFurtherCurrencyProvider provider)
        {
            _memoryCache = memoryCache;
            _provider = provider;
        }

        public async Task<PaginatedHistoricalRatesOutputDto> Handle(GetHistoricalExchangeRatesQuery query, CancellationToken cancellationToken)
        {
            var allRates = await _provider.GetHistoricalRatesAsync(query.BaseCurrency, query.StartDate, query.EndDate);

            var totalCount = allRates.Count;
            var skip = (query.Page - 1) * query.PageSize;

            var paged = allRates
                .OrderBy(x => x.Date)
                .Skip(skip)
                .Take(query.PageSize)
                .ToList();

            return new PaginatedHistoricalRatesOutputDto
            {
                BaseCurrency = query.BaseCurrency,
                StartDate = query.StartDate,
                EndDate = query.EndDate,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                Rates = paged
            };
        }
    }
}
