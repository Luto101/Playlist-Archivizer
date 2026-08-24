using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    public class UserResponse
    {
        [JsonPropertyName("account_id")]
        public string AccountId { get; init; } = default!;
    }
}