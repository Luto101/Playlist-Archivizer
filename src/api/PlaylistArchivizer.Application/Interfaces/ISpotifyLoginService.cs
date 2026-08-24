using PlaylistArchivizer.Application.Dtos;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyLoginService
    {
        /// <summary>Generates the official Spotify OAuth authorization URL.</summary>
        string GenerateLoginUrl(string state);

        /// <summary>Exchanges the authorization code for access and refresh tokens.</summary>
        Task<SpotifyUserDataDto> AuthenticateAsync(string code);

        /// <summary>Refreshes an expired access token using the refresh token.</summary>
        Task<SpotifyTokenDto> RefreshTokenAsync(string refreshToken);
    }
}
