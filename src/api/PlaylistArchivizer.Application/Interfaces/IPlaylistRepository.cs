using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<List<Playlist>> GetArchivedPlaylistsAsync(string userId, CancellationToken token = default);

        Task AddPlaylistAsync(Playlist playlist, string userId, CancellationToken token = default);

        Task UpdatePlaylistTracksAsync(string playlistId, string snapshotId, IEnumerable<Track> newTracks, string userId, CancellationToken token = default);

        Task RemovePlaylistAsync(string playlistId, string userId, CancellationToken token = default);

        Task RemoveTrackAsync(string playlistId, string trackId, string userId, CancellationToken token = default);
    }
}
