namespace CurrencyExchange.Application.Interfaces
{
    public interface ICurrencyValidator
    {
        HashSet<string> ValidCurrencies { get; }
        HashSet<string> ForbiddenCurrencies { get; }
    }
}
