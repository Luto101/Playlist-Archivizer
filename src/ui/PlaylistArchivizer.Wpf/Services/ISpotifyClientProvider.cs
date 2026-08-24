using PlaylistArchivizer.UI.Core;

namespace PlaylistArchivizer.Wpf.Services
{
    public interface ISpotifyClientProvider
    {
        SpotifyClient Client { get; }
        Task InitializeAsync();
    }
}
