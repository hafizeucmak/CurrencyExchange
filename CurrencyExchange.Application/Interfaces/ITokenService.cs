namespace CurrencyExchange.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(string email, string clientId, string userRole);
    }
}
