using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifySavedTrackService
    {
        Task<IEnumerable<string>> GetUserSavedTracksIdsAsync(CancellationToken token = default);
    }
}
