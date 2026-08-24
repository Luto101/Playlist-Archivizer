using PlaylistArchivizer.Domain.Entities;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyTokenRepository
    {
        Task<SpotifyCredential?> GetByUserIdAsync(string userId);
        Task UpsertAsync(SpotifyCredential newToken);
    }
}