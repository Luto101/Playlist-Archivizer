using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyTrackService
    {
        Task<IEnumerable<Track>> GetPlaylistTracksAsync(string playlistId, CancellationToken token = default);
        Task<IEnumerable<Track>> GetTrackInfoFromIdsAsync(Queue<string> ids, CancellationToken token = default);
    }
}
