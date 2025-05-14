namespace CurrencyExchange.Application.Abstractions.Providers.FrankFurter
{
    public class FrankFurterHistoricalResponse
    {
        public string Base { get; set; } = default!;
        public string Start_Date { get; set; } = default!;
        public string End_Date { get; set; } = default!;
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
    }
}
