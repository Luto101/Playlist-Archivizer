using Microsoft.IdentityModel.Tokens;
using PlaylistArchivizer.API.Entities;
using PlaylistArchivizer.API.Models;
using PlaylistArchivizer.API.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlaylistArchivizer.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISpotifyLoginService _spotifyLoginService;
        private readonly ISpotifyTokenRepository _tokenRepository;
        private readonly IConfiguration _config;

        public AuthService(ISpotifyLoginService spotifyLoginService, ISpotifyTokenRepository tokenRepository, IConfiguration config)
        {
            _spotifyLoginService = spotifyLoginService;
            _tokenRepository = tokenRepository;
            _config = config;
        }

        public async Task<string> ProcessSpotifyLoginAsync(string code)
        {
            SpotifyUserData userData = await _spotifyLoginService.AuthenticateAsync(code);

            string token = GenerateToken(userData.UserId);

            await _tokenRepository.UpsertAsync(new SpotifyToken
            {
                UserId = userData.UserId,
                AccessToken = userData.AccessToken,
                RefreshToken = userData.RefreshToken,
                ExpiresIn = userData.ExpiresIn,
                CreatedAt = DateTime.UtcNow
            });

            return token;
        }

        private string GenerateToken(string userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.CreateToken(token);

            return tokenHandler.WriteToken(jwt);
        }
    }
}
