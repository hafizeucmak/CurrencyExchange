namespace CurrencyExchange.Application.Abstractions.Providers
{
    public class IFrankFurtherCurrencyProvider : ICurrencyProvider
    {
        public string Name => "FrankFurter";

        public Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            throw new NotImplementedException();
        }
    }
}
