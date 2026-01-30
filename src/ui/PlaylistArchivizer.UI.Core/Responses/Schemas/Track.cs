namespace PlaylistArchivizer.UI.Core.Responses.Schemas
{
    public class Track
    {
        public Album album { get; set; } = default!;
        public Artist[] artists { get; set; } = default!;
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string type { get; set; } = default!; // "episode" or "track"
        public bool is_local { get; set; }
    }

    public class Album
    {
        public Image[] images { get; set; } = default!;
    }

    public class Artist
    {
        public string name { get; set; } = default!;
    }
}
