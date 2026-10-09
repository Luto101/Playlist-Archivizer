using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifyTrackService
    {
        Task<IReadOnlyCollection<Track>> GetPlaylistTracksAsync(string playlistId, CancellationToken token = default);
        Task<IReadOnlyCollection<Track>> GetTrackInfoFromIdsAsync(IEnumerable<string> ids, CancellationToken token = default);
    }
}
