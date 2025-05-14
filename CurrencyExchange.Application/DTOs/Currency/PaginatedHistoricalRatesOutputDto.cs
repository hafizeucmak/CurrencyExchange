namespace CurrencyExchange.Application.DTOs.Currency
{
    public class PaginatedHistoricalRatesOutputDto
    {
        public string BaseCurrency { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public List<HistoricalRateRecords> Rates { get; set; } = new();
    }

    public class HistoricalRateRecords
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
