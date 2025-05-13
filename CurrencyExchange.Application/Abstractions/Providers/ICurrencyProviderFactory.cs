namespace CurrencyExchange.Application.Abstractions.Providers
{
    public interface ICurrencyProviderFactory
    {
        ICurrencyProvider GetProvider(string providerName);
    }
}
