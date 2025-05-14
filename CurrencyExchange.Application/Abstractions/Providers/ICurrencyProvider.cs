using CurrencyExchange.Application.DTOs.Currency;

namespace CurrencyExchange.Application.Abstractions.Providers
{
    public interface ICurrencyProvider
    {
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);

        Task<ExchangeRatesDto> GetLatestRatesAsync(string baseCurrency);

        Task<List<HistoricalRateRecords>> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end);

        string Name { get; }
    }
}
