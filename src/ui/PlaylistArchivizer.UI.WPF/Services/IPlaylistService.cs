using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.UI.WPF.ViewModels.Home;

namespace PlaylistArchivizer.UI.WPF.Services
{
    public interface IPlaylistService
    {
        Task<List<TrackViewModel>> GetFreeTracksAsync(CancellationToken token);
        IEnumerable<TrackViewModel> GetVMsFromPlaylist(Playlist playlist, Action<TrackViewModel> removeCallback);
        void UpdatePlaylistViewAfterSavedTracksSync(IEnumerable<TrackViewModel> currentTracks);
    }
}
