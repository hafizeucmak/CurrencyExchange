namespace CurrencyExchange.Infrastructure.Configurations
{
    public class ConfigurationOptions
    {
        public required JwtSettingsOptions JwtSettings { get; set; }
        public required DbConnectionOptions DbConnectionOptions { get; set; }
    }
}
