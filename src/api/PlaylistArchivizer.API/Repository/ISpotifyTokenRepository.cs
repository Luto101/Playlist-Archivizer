using PlaylistArchivizer.API.Entities;

namespace PlaylistArchivizer.API.Repository
{
    public interface ISpotifyTokenRepository
    {
        Task<SpotifyToken?> GetByUserIdAsync(string userId);
        Task UpsertAsync(SpotifyToken newToken);
    }
}