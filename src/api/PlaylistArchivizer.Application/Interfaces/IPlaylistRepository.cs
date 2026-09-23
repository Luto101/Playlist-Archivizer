using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<List<Playlist>> GetArchivedPlaylistsAsync(CancellationToken token);
        Task AddPlaylistAsync(Playlist playlist, CancellationToken token);
        Task UpdatePlaylistTracksAsync(string playlistId, string snapshotId, IEnumerable<Track> newTracks, CancellationToken token);
        Task RemovePlaylistAsync(string playlistId, CancellationToken token);
        Task RemoveTrackAsync(string playlistId, string trackId, CancellationToken token);
    }
}
