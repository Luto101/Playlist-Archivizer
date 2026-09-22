using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses.Schemas;
using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    // Fields: tracks(album.images, artists.name, id, name, type, is_local)
    public class GetTracksResponse
    {
        [JsonPropertyName("tracks")]
        public SpotifyTrack[] Tracks { get; set; } = default!;
    }
}
