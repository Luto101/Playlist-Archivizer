using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = default!;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = default!;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = default!;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = default!;
    }
}