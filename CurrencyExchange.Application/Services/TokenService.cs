using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyExchange.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly ConfigurationOptions _configurationOptions;
        public TokenService(IOptions<ConfigurationOptions> configurationOpitons)
        {
            _configurationOptions = configurationOpitons.Value;
        }

        public Task<string> GenerateToken(string email, string clientId, string userRole)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configurationOptions.JwtSettings.SecretKey);

            var claims = new List<Claim>
                        {
                            new Claim("email", email),
                            new Claim("clientId", clientId)
                        };

            claims.Add(new Claim(ClaimTypes.Role, userRole));

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _configurationOptions.JwtSettings.ValidIssuer,
                audience: _configurationOptions.JwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
