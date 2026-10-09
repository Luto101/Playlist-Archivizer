using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifySavedTrackService
    {
        Task<IReadOnlyCollection<string>> GetUserSavedTracksIdsAsync(CancellationToken token = default);
    }
}
