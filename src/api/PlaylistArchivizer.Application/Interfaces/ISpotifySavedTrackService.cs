using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Interfaces
{
    public interface ISpotifySavedTrackService
    {
        Task<IReadOnlyCollection<string>> GetUserSavedTrackIdsAsync(CancellationToken token = default);
        Task<IReadOnlyCollection<string>> GetSavedTrackIdsAsync(IEnumerable<string> trackIds, CancellationToken token = default);
    }
}
