namespace PlaylistArchivizer.UI.Core.Models
{
    public class Playlist
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SnapshotId { get; set; }
        public List<Track> Tracks { get; set; }

        public Playlist(string id, string name, string snapshotId, List<Track>? tracks = null)
        {
            Id = id;
            Name = name;
            SnapshotId = snapshotId;

            if (tracks != null)
                Tracks = tracks;
            else
                Tracks = [];
        }
    }
}
