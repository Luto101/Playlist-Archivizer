using PlaylistArchivizer.API.Entities;

namespace PlaylistArchivizer.API.Repositories
{
    public interface ISpotifyTokenRepository
    {
        Task<SpotifyToken?> GetByUserIdAsync(string userId);
        Task UpsertAsync(SpotifyToken newToken);
    }
}