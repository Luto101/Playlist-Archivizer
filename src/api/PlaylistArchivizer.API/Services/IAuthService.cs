namespace PlaylistArchivizer.API.Services
{
    public interface IAuthService
    {
        Task<string> GetValidSpotifyTokenAsync(string userId);
        Task<string> ProcessSpotifyLoginAsync(string code);
    }
}