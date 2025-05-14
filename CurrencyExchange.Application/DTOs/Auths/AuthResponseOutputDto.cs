namespace CurrencyExchange.Application.DTOs.Auths
{
    public class AuthResponseOutputDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
