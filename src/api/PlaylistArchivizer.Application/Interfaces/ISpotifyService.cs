using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyService
    {
        Task<IReadOnlyCollection<Playlist>> GetPlaylistsAsync(CancellationToken token = default);
        Task RemoveTrackAsync(string playlistId, string trackId, CancellationToken token = default);
        Task RemovePlaylistAsync(string playlistId, CancellationToken token = default);
        Task CreateArchivedPlaylistInSpotify(string playlistId, CancellationToken token = default);
        Task<IReadOnlyCollection<Track>> GetFreeTracksAsync(CancellationToken token = default);
        Task<IReadOnlyCollection<string>> GetSavedTrackIdsFromPlaylistsAsync(CancellationToken token = default);
    }
}
