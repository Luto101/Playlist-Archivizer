namespace PlaylistArchivizer.Application.Interfaces
{
    public interface IIgnoredPlaylistRepository
    {
        Task<HashSet<string>> GetIgnoredPlaylistIdsAsync(CancellationToken token);
        Task AddToIgnoredAsync(string playlistId, CancellationToken token);
    }
}
