using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses.Schemas;
using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    // Fields: total, items(added_at, track(album.images, artists.name, id, name, type, is_local))
    public class PlaylistItemsResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public Item[] Items { get; set; } = default!;

        public class Item
        {
            [JsonPropertyName("added_at")]
            public DateTime? AddedAt { get; set; } // Some very old playlists may return null

            [JsonPropertyName("track")]
            public SpotifyTrack Track { get; set; } = default!;
        }
    }
}
