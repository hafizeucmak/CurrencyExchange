namespace CurrencyExchange.Application.Abstractions.Providers
{
    public interface ICurrencyProvider
    {
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        string Name { get; }
    }
}
