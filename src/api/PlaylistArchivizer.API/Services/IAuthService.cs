namespace PlaylistArchivizer.API.Services
{
    public interface IAuthService
    {
        Task<string> ProcessSpotifyLoginAsync(string code);
    }
}