using PlaylistArchivizer.Domain.Entities;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyCredentialRepository
    {
        Task<SpotifyCredential?> GetByUserIdAsync(string userId);
        Task UpsertAsync(SpotifyCredential newToken);
    }
}