using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.Wpf.ViewModels.Home;

namespace PlaylistArchivizer.Wpf.Services
{
    public interface IPlaylistService
    {
        Task<List<TrackViewModel>> GetFreeTracksAsync(CancellationToken token);
        IEnumerable<TrackViewModel> GetVMsFromPlaylist(Playlist playlist, Action<TrackViewModel> removeCallback);
        void UpdatePlaylistViewAfterSavedTracksSync(IEnumerable<TrackViewModel> currentTracks);
    }
}
