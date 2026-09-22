using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses.Schemas;
using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    // Fields: id, images, name, snapshot_id
    public class PlaylistResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("images")]
        public SpotifyImage[] Images { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("snapshot_id")]
        public string SnapshotId { get; set; } = default!;
    }
}
