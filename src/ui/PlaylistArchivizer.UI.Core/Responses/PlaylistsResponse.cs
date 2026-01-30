namespace PlaylistArchivizer.UI.Core.Responses
{
    // Fields: total, items(id, images, name, snapshot_id)
    public class PlaylistsResponse
    {
        public int total { get; set; }
        public PlaylistResponse[] items { get; set; } = default!;
    }

}
