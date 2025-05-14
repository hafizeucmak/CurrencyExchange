namespace CurrencyExchange.Infrastructure.Configurations
{
    public class CurrencyProviderOptions
    {
        public required FrankFurterProvider FrankFurter { get; set; }
    }

    public class FrankFurterProvider
    {
        public required string BaseUrl { get; set; }
    }
}
