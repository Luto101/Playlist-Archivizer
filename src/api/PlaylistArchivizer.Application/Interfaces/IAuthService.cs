using PlaylistArchivizer.Application.Dtos;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>Returns a valid Spotify access token, automatically refreshing it if expired.</summary>
        Task<string> GetValidSpotifyTokenAsync(string userId);

        /// <summary>Handles the full Spotify login flow by authenticating the user and saving credentials.</summary>
        Task<SpotifyUserDataDto> ProcessSpotifyLoginAsync(string code);
    }
}
