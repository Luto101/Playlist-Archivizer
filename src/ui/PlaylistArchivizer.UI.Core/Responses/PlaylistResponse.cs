using PlaylistArchivizer.UI.Core.Responses.Schemas;

namespace PlaylistArchivizer.UI.Core.Responses
{
    // Fields: id, images, name, snapshot_id
    public class PlaylistResponse
    {
        public string id { get; set; } = default!;
        public Image[] images { get; set; } = default!;
        public string name { get; set; } = default!;
        public string snapshot_id { get; set; } = default!;
    }

}
