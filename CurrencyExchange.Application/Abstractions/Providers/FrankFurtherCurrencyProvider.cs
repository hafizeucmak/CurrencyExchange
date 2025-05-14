using CurrencyExchange.Application.Abstractions.Providers.FrankFurter;
using CurrencyExchange.Application.DTOs.Currency;
using System.Net.Http.Json;
using System.Text.Json;

namespace CurrencyExchange.Application.Abstractions.Providers
{
    public class FrankFurtherCurrencyProvider : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;

        public FrankFurtherCurrencyProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public string Name => "FrankFurter";

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            var url = $"/latest?base={fromCurrency}&symbols={toCurrency}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var rate = json.RootElement.GetProperty("rates").GetProperty(toCurrency.ToUpper()).GetDecimal();

            return rate;
        }

        public async Task<ExchangeRatesDto> GetLatestRatesAsync(string baseCurrency)
        {
            var url = $"/latest?base={baseCurrency}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var raw = JsonSerializer.Deserialize<FrankFurterRawResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new ExchangeRatesDto
            {
                Amount = raw.Amount,
                BaseCurrency = raw.Base,
                Date = raw.Date,
                Rates = raw.Rates
            };
        }

        public async Task<List<HistoricalRateRecords>> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end)
        {
            string formattedStart = start.ToString("yyyy-MM-dd");
            string formattedEnd = end.ToString("yyyy-MM-dd");

            var url = $"{formattedStart}..{formattedEnd}?base={baseCurrency}";
            var response = await _httpClient.GetFromJsonAsync<FrankFurterHistoricalResponse>(url);

            if (response == null || response.Rates == null)
                throw new Exception("Failed to retrieve historical exchange rates.");

            return response.Rates
                .Select(rate => new HistoricalRateRecords
                {
                    Date = DateTime.Parse(rate.Key),
                    Rates = rate.Value
                })
                .ToList();
        }
    }
}
