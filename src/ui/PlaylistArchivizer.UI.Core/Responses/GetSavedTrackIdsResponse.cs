namespace PlaylistArchivizer.UI.Core.Responses
{
    // Fields: total, items.track.id
    public class GetSavedTrackIdsResponse
    {
        public int total { get; set; }
        public Item[] items { get; set; } = default!;

        public class Item
        {
            public Track track { get; set; } = default!;
        }

        public class Track
        {
            public string id { get; set; } = default!;
        }
    }
}
