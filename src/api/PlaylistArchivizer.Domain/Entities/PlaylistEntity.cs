namespace PlaylistArchivizer.Domain.Entities
{
    public class PlaylistEntity
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string SnapshotId { get; set; } = default!;
        public string UserId { get; set; } = default!;

        public List<TrackEntity> Tracks { get; set; } = [];
    }
}
