using PlaylistArchivizer.UI.Core;

namespace PlaylistArchivizer.UI.WPF.Services
{
    public interface ISpotifyClientProvider
    {
        SpotifyClient Client { get; }
        Task InitializeAsync();
    }
}
