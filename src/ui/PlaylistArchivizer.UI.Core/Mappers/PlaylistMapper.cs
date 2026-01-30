using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.UI.Core.Responses;

namespace PlaylistArchivizer.UI.Core.Mappers
{
    public static class PlaylistMapper
    {
        public static Playlist Map(PlaylistResponse item, List<Track> tracks) =>
            new(item.id, item.name, item.snapshot_id, tracks);

        public static Playlist Map(PlaylistResponse playlist) =>
            new(playlist.id, playlist.name, playlist.snapshot_id);
    }
}
