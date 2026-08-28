using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PlaylistArchivizer.Application.Interfaces;
using System.Security.Claims;
using System.Text;

namespace PlaylistArchivizer.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly SigningCredentials _credentials;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration config)
        {
            _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing.");
            _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing.");

            string? secret = config["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret) || secret.Length < 32)
                throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secret));
            _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateToken(string userId)
        {
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Claims = new Dictionary<string, object>
                {
                    { ClaimTypes.NameIdentifier, userId }
                },
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = _credentials
            };

            JsonWebTokenHandler tokenHandler = new();
            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}
