using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlaylistArchivizer.UI.Core;
using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.UI.Core.Persistence;
using System.Windows;

namespace PlaylistArchivizer.UI.WPF.ViewModels.Home
{
    /// <summary>
    /// ViewModel of single row in DataGrid representing a Track
    /// </summary>
    public partial class TrackViewModel : ObservableObject
    {
        private readonly SpotifyClient _client;
        private readonly Action<TrackViewModel>? _onRemove;
        private readonly Playlist? playlist;
        private readonly Track track;

        public string Name { get; private set; }
        public DateTime? AddedAt { get; private set; }
        public string ArtistsString { get; private set; }

        [ObservableProperty] private bool? isSaved;
        [ObservableProperty] private string? imagePath;

        public TrackViewModel(Track track, SpotifyClient client, Playlist? playlist = null, Action<TrackViewModel>? onRemove = null)
        {
            _client = client;
            _onRemove = onRemove;

            this.playlist = playlist;
            this.track = track;

            Name = track.Name;
            AddedAt = track.AddedAt;
            IsSaved = track.IsSaved;
            ArtistsString = string.Join(", ", track.Artists);

            // Load image in a background
            Task.Run(() => LoadImageAysnc(track));
        }

        private async Task LoadImageAysnc(Track track) => ImagePath = await ImageStorage.GetImagePathAsync(track);

        public void ReloadIsSavedProperty() => IsSaved = playlist?.Tracks.First(x => x == track).IsSaved;

        [RelayCommand]
        public async Task RemoveTrack()
        {
            if (_onRemove == null || playlist == null)
                return;

            MessageBoxResult result = MessageBox.Show($"Czy napewno chcesz usunąć utwór: {Name} z historii playlisty?", "Ostrzeżenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _client.RemoveTrackAsync(playlist, track);

                _onRemove.Invoke(this);
            }
        }
    }
}
