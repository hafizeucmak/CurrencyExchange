namespace CurrencyExchange.Application.DTOs.Currency
{
    public class ExchangeRatesDto
    {
        public decimal Amount { get; set; }
        public string BaseCurrency { get; set; } = default!;
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
