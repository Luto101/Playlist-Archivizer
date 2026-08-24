using PlaylistArchivizer.UI.Core;

namespace PlaylistArchivizer.Wpf.Services
{
    public class SpotifyClientProvider : ISpotifyClientProvider
    {
        public SpotifyClient Client { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            Client = await SpotifyClient.CreateAsync();
        }
    }
}
