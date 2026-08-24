namespace PlaylistArchivizer.Domain.Entities
{
    public class SpotifyCredential
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public bool IsAccessTokenValid => CreatedAt.AddSeconds(ExpiresIn - 60) > DateTimeOffset.UtcNow; // 60 seconds buffer
    }
}
