using CurrencyExchange.Application.Interfaces;
using System.Text.Json;

namespace CurrencyExchange.API.Filters
{
    public class CurrencyConfig
    {
        public required List<string> ValidCurrencies { get; set; }
        public required List<string> ExcludedCurrencies { get; set; }
    }

    public class CurrencyValidator : ICurrencyValidator
    {
        public HashSet<string> ValidCurrencies { get; }
        public HashSet<string> ForbiddenCurrencies { get; }

        public CurrencyValidator(IWebHostEnvironment env)
        {
            var filePath = Path.Combine(env.WebRootPath, "valid-currencies.json");
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<CurrencyConfig>(json);

            if (config == null)
            {
                throw new InvalidOperationException("Failed to load currency configuration.");
            }

            ValidCurrencies = new HashSet<string>(config.ValidCurrencies ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            ForbiddenCurrencies = new HashSet<string>(config.ExcludedCurrencies ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
