using PlaylistArchivizer.API.Models;
using PlaylistArchivizer.API.Responses;

namespace PlaylistArchivizer.API.Services
{
    public interface ISpotifyLoginService
    {
        Task<SpotifyUserData> AuthenticateAsync(string code);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    }
}