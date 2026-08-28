using PlaylistArchivizer.Application.Dtos;
using PlaylistArchivizer.Application.Exceptions;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;

namespace PlaylistArchivizer.Application.Services
{
    public class AuthService(ISpotifyLoginService spotifyLoginService,
                             ISpotifyTokenRepository tokenRepository) : IAuthService
    {
        public async Task<SpotifyUserDataDto> ProcessSpotifyLoginAsync(string code)
        {
            SpotifyUserDataDto userData = await spotifyLoginService.AuthenticateAsync(code);

            await tokenRepository.UpsertAsync(new SpotifyCredential
            {
                UserId = userData.UserId,
                AccessToken = userData.Token.AccessToken,
                RefreshToken = userData.Token.RefreshToken,
                ExpiresIn = userData.Token.ExpiresIn,
                CreatedAt = DateTimeOffset.UtcNow
            });

            return userData;
        }

        public async Task<string> GetValidSpotifyTokenAsync(string userId)
        {
            var spotifyCredential = await tokenRepository.GetByUserIdAsync(userId) ??
                throw new NotFoundException("Spotify credentials", userId);

            if (spotifyCredential.IsAccessTokenValid)
                return spotifyCredential.AccessToken;

            // Token is expired. Call for a new access token using the refresh token
            SpotifyTokenDto newToken = await spotifyLoginService.RefreshTokenAsync(spotifyCredential.RefreshToken);

            spotifyCredential.AccessToken = newToken.AccessToken;
            spotifyCredential.ExpiresIn = newToken.ExpiresIn;
            spotifyCredential.CreatedAt = DateTimeOffset.UtcNow;

            // Spotify sometimes issues a new refresh token
            if (!string.IsNullOrEmpty(newToken.RefreshToken))
                spotifyCredential.RefreshToken = newToken.RefreshToken;

            await tokenRepository.UpsertAsync(spotifyCredential);

            return spotifyCredential.AccessToken;
        }
    }
}
