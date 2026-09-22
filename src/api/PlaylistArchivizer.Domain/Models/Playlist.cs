namespace PlaylistArchivizer.Domain.Models
{
    public class Playlist
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string SnapshotId { get; init; }
        public List<Track> Tracks { get; init; } = [];

        public Playlist(string id, string name, string snapshotId, IEnumerable<Track>? tracks = null)
        {
            Id = id;
            Name = name;
            SnapshotId = snapshotId;

            if (tracks != null)
                Tracks.AddRange(tracks);
        }
    }
}
