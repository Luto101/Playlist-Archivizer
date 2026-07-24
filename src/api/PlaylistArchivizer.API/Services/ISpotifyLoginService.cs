using PlaylistArchivizer.API.Models;

namespace PlaylistArchivizer.API.Services
{
    public interface ISpotifyLoginService
    {
        Task<SpotifyUserData> AuthenticateAsync(string code);
    }
}