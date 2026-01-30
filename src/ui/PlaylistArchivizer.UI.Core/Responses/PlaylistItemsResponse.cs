using PlaylistArchivizer.UI.Core.Responses.Schemas;

namespace PlaylistArchivizer.UI.Core.Responses
{
    // Fields: total, items(added_at, track(album.images, artists.name, id, name, type, is_local))
    public class PlaylistItemsResponse
    {
        public int total { get; set; }
        public Item[] items { get; set; } = default!;


        public class Item
        {
            public DateTime? added_at { get; set; } // Some very old playlists may return null
            public Track track { get; set; } = default!;
        }
    }
}
