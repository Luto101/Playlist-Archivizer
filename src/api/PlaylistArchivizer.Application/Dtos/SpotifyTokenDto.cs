namespace PlaylistArchivizer.Application.Dtos
{
    public class SpotifyTokenDto
    {
        public string AccessToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = default!;
    }
}
