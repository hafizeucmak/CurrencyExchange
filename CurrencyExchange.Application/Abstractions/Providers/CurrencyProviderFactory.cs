namespace CurrencyExchange.Application.Abstractions.Providers
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IEnumerable<ICurrencyProvider> _providers;

        public CurrencyProviderFactory(IEnumerable<ICurrencyProvider> providers)
        {
            _providers = providers;
        }

        public ICurrencyProvider GetProvider(string providerName)
        {
            var provider = _providers.FirstOrDefault(p => string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                throw new ArgumentException($"Currency provider not found: {providerName}");
            }

            return provider;
        }
    }
}
