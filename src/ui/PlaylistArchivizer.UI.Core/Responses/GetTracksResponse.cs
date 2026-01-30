using PlaylistArchivizer.UI.Core.Responses.Schemas;

namespace PlaylistArchivizer.UI.Core.Responses
{
    // Fields: tracks(album.images, artists.name, id, name, type, is_local)
    public class GetTracksResponse
    {
        public Track[] tracks { get; set; } = default!;
    }
}
