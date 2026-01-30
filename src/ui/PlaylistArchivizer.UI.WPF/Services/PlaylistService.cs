using PlaylistArchivizer.UI.Core;
using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.UI.WPF.ViewModels.Home;

namespace PlaylistArchivizer.UI.WPF.Services
{
    public class PlaylistService(ISpotifyClientProvider spotifyProvider) : IPlaylistService
    {
        private readonly SpotifyClient _client = spotifyProvider.Client;
        private List<TrackViewModel>? _cachedFreeTracks;

        public IEnumerable<TrackViewModel> GetVMsFromPlaylist(Playlist playlist, Action<TrackViewModel> removeCallback)
        {
            return playlist.Tracks.Select(t => new TrackViewModel(t, _client, playlist, removeCallback));
        }

        public async Task<List<TrackViewModel>> GetFreeTracksAsync(CancellationToken token)
        {
            if (_cachedFreeTracks != null)
                return _cachedFreeTracks;

            var allTrackIdsInPlaylists = _client.Playlists
                .SelectMany(p => p.Tracks)
                .Select(t => t.Id)
                .ToHashSet();

            var allSavedTracksIds = await _client.GetUserSavedTracksIdsAsync(token);
            var difference = allSavedTracksIds.Except(allTrackIdsInPlaylists).ToList();

            var tracks = await _client.GetTrackInfoFromIds(new(difference));

            var results = tracks.Select(t =>
            {
                t.IsSaved = true;
                return new TrackViewModel(t, _client);
            }).ToList();

            // Cashe result
            if (!token.IsCancellationRequested)
                _cachedFreeTracks = results;

            return results;
        }

        public void UpdatePlaylistViewAfterSavedTracksSync(IEnumerable<TrackViewModel> currentTracks)
        {
            foreach (var track in currentTracks)
                track.ReloadIsSavedProperty();
        }
    }
}
