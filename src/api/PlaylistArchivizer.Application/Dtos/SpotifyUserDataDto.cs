namespace PlaylistArchivizer.Application.Dtos
{
    public class SpotifyUserDataDto
    {
        public string UserId { get; set; } = default!;
        public SpotifyTokenDto Token { get; set; } = default!;
    }
}