namespace PlaylistArchivizer.Domain.Entities
{
    public class IgnoredPlaylist
    {
        public int Id { get; set; }
        public string PlaylistId { get; set; } = default!;
        public string UserId { get; set; } = default!;
    }
}
