namespace PlaylistArchivizer.Domain.Entities
{
    public class TrackEntity
    {
        public string Id { get; set; } = default!;
        public string PlaylistId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public DateTime? AddedAt { get; set; }
        public string ImageUrl { get; set; } = default!;

        public List<string> Artists { get; set; } = [];
    }
}
