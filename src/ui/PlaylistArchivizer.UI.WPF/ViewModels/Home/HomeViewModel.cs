using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlaylistArchivizer.UI.Core;
using PlaylistArchivizer.UI.WPF.Services;
using System.Collections.ObjectModel;

namespace PlaylistArchivizer.UI.WPF.ViewModels.Home
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly SpotifyClient _client;
        private readonly IPlaylistService _playlistService;
        private CancellationTokenSource? _playlistLoadingCts;

        [ObservableProperty] private ObservableCollection<PlaylistViewModel> playlists = [];
        [ObservableProperty] private ObservableCollection<TrackViewModel> currentDisplayedTracks = [];
        [ObservableProperty] private DisplayedTracksState displayedTracksState = DisplayedTracksState.None;
        [ObservableProperty] private bool isFreeTracksSelected;

        public HomeViewModel(ISpotifyClientProvider spotifyClientProvider, IPlaylistService playlistService)
        {
            _client = spotifyClientProvider.Client;
            _playlistService = playlistService;

            foreach (var p in _client.Playlists)
                Playlists.Add(new(p, SelectPlaylist, RemovePlaylist, _client));

            // Lazy load is saved state
            Task.Run(InitializeIsSavedStatusAsync);
        }

        private async Task InitializeIsSavedStatusAsync()
        {
            await _client.UpdateSavedTracksAsync();

            if (DisplayedTracksState == DisplayedTracksState.Playlist)
                _playlistService.UpdatePlaylistViewAfterSavedTracksSync(CurrentDisplayedTracks);
        }

        // Updates view on playlist change
        public async Task SelectPlaylist(PlaylistViewModel clickedPlaylist)
        {
            CancellationToken token = CancelAndCreateNewToken();
            bool wasSelected = clickedPlaylist.IsSelected;

            ClearDisplayedTracks();
            clickedPlaylist.IsSelected = !wasSelected;

            // Unclicking
            if (!clickedPlaylist.IsSelected)
                return;

            DisplayedTracksState = DisplayedTracksState.Loading;

            var playlist = _client.Playlists.First(x => x.Id == clickedPlaylist.Id);
            var trackVMs = _playlistService.GetVMsFromPlaylist(playlist, RemoveTrack);

            foreach (var trackVM in trackVMs)
            {
                if (token.IsCancellationRequested)
                    break;

                CurrentDisplayedTracks.Add(trackVM);
            }

            DisplayedTracksState = DisplayedTracksState.Playlist;
            CheckIfTracksAreEmpty();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SelectFreeTracks()
        {
            CancellationToken token = CancelAndCreateNewToken();
            bool wasSelected = IsFreeTracksSelected;

            ClearDisplayedTracks();

            // Unclicking
            if (wasSelected)
                return;

            IsFreeTracksSelected = true;
            DisplayedTracksState = DisplayedTracksState.Loading;

            var tracks = await _playlistService.GetFreeTracksAsync(token);

            if (token.IsCancellationRequested)
                return;

            tracks.ForEach(CurrentDisplayedTracks.Add);
            DisplayedTracksState = DisplayedTracksState.FreeTracks;
            CheckIfTracksAreEmpty();
        }

        public void RemovePlaylist(PlaylistViewModel playlist)
        {
            Playlists.Remove(playlist);

            if (playlist.IsSelected)
                ClearDisplayedTracks();
        }

        public void RemoveTrack(TrackViewModel track)
        {
            CurrentDisplayedTracks.Remove(track);
            CheckIfTracksAreEmpty();
        }

        private void ClearDisplayedTracks()
        {
            DisplayedTracksState = DisplayedTracksState.None;
            IsFreeTracksSelected = false;
            CurrentDisplayedTracks = [];

            foreach (var p in Playlists)
                p.IsSelected = false;
        }

        private void CheckIfTracksAreEmpty()
        {
            if (!CurrentDisplayedTracks.Any())
                DisplayedTracksState = DisplayedTracksState.Empty;
        }

        private CancellationToken CancelAndCreateNewToken()
        {
            _playlistLoadingCts?.Cancel();
            _playlistLoadingCts = new CancellationTokenSource();
            return _playlistLoadingCts.Token;
        }
    }
}
