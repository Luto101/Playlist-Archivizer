using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    // Fields: total, items(id, images, name, snapshot_id)
    public class PlaylistsResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public PlaylistResponse[] Items { get; set; } = default!;
    }

}
