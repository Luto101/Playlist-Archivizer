using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    // Fields: total, items.track.id
    public class GetSavedTrackIdsResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public Item[] Items { get; set; } = default!;

        public class Item
        {
            [JsonPropertyName("track")]
            public Track Track { get; set; } = default!;
        }

        public class Track
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = default!;
        }
    }
}
