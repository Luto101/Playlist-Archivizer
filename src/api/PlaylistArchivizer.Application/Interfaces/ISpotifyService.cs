using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyService
    {
        Task<IEnumerable<Playlist>> GetPlaylistsAsync(CancellationToken token = default);
        Task RemoveTrackAsync(Playlist playlist, string trackId, CancellationToken token = default);
        Task RemovePlaylistAsync(string playlistId, CancellationToken token = default);
        Task CreatePlaylistWithTracksAsync(string name, IEnumerable<Track> tracks, CancellationToken token = default);
        Task<IEnumerable<string>> GetUserSavedTracksIdsAsync(CancellationToken token);
        Task<IEnumerable<Track>> GetTrackInfoFromIdsAsync(Queue<string> ids, CancellationToken token = default);
    }
}
