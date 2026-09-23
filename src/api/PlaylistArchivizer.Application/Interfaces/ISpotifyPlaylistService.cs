using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyPlaylistService
    {
        Task<IEnumerable<Playlist>> GetPlaylistsAsync(CancellationToken token = default);
        Task<Playlist> CreatePlaylistAsync(string name, CancellationToken token = default);
        Task AddTracksToPlaylistAsync(Playlist playlist, IEnumerable<Track> tracks, CancellationToken token = default);
    }
}
