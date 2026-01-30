using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlaylistArchivizer.UI.Core;
using PlaylistArchivizer.UI.Core.Models;
using System.Windows;

namespace PlaylistArchivizer.UI.WPF.ViewModels.Home
{
    /// <summary>
    /// ViewModel of single ToggleButton representing a Playlist
    /// </summary>
    public partial class PlaylistViewModel(Playlist playlist, Func<PlaylistViewModel, Task> onSelect, Action<PlaylistViewModel> onRemove, SpotifyClient client) : ObservableObject
    {
        private readonly SpotifyClient _client = client;

        public string Name { get; set; } = playlist.Name;
        public string Id { get; set; } = playlist.Id;

        [ObservableProperty] private bool isSelected;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task Select() => await onSelect.Invoke(this);

        [RelayCommand]
        private async Task RemovePlaylist()
        {
            MessageBoxResult result = MessageBox.Show($"Czy napewno chcesz usunąć playliste: {Name} z historii playlist?", "Ostrzeżenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _client.RemovePlaylistAsync(playlist);
                onRemove.Invoke(this);
            }
        }

        [RelayCommand]
        private async Task CreatePlaylist()
        {
            string name = $"Archive: {Name}";
            await _client.CeatePlaylistWithTracksAsync(name, playlist.Tracks);

            MessageBox.Show($"Utworzono na Spotify playlistę: \"{name}\"");
        }
    }
}
