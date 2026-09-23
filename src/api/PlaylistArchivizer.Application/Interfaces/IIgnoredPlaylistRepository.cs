namespace PlaylistArchivizer.Application.Interfaces
{
    public interface IIgnoredPlaylistRepository
    {
        Task<HashSet<string>> GetIgnoredPlaylistIdsAsync(string userId, CancellationToken token = default);
        Task AddToIgnoredAsync(string playlistId, string userId, CancellationToken token = default);
    }
}
