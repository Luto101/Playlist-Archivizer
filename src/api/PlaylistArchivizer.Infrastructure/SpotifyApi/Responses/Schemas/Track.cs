using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses.Schemas
{
    public class SpotifyTrack
    {
        [JsonPropertyName("album")]
        public SpotifyAlbum Album { get; set; } = default!;

        [JsonPropertyName("artists")]
        public SpotifyArtist[] Artists { get; set; } = default!;

        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = default!; // "episode" or "track"

        [JsonPropertyName("is_local")]
        public bool IsLocal { get; set; }
    }

    public class SpotifyAlbum
    {
        [JsonPropertyName("images")]
        public SpotifyImage[] Images { get; set; } = default!;
    }

    public class SpotifyArtist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }
}
