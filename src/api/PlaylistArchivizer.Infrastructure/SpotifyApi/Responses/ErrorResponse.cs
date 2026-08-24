using System.Text.Json.Serialization;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public SpotifyError Error { get; init; } = default!;
}

public class SpotifyError
{
    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;
}