using PlaylistArchivizer.Domain.Models;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Mappers
{
    public static class PlaylistMapper
    {
        public static Playlist Map(PlaylistResponse playlist, IEnumerable<Track> tracks) =>
            new(playlist.Id, playlist.Name, playlist.SnapshotId, tracks);

        public static Playlist Map(PlaylistResponse playlist) =>
            new(playlist.Id, playlist.Name, playlist.SnapshotId);
    }
}
