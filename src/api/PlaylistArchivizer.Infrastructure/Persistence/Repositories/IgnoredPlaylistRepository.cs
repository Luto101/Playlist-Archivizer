using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Infrastructure.Persistence.Data;

namespace PlaylistArchivizer.Infrastructure.Persistence.Repositories
{
    public class IgnoredPlaylistRepository(ApplicationDbContext dbContext) : IIgnoredPlaylistRepository
    {
        public async Task<HashSet<string>> GetIgnoredPlaylistIdsAsync(string userId, CancellationToken token = default)
        {
            // Fetch only the ignored playlist IDs that belong to the user
            var ids = await dbContext.IgnoredPlaylists
                .Where(p => p.UserId == userId)
                .Select(p => p.PlaylistId)
                .ToListAsync(token);

            return ids.ToHashSet();
        }

        public async Task AddToIgnoredAsync(string playlistId, string userId, CancellationToken token = default)
        {
            // Check if this playlist is already marked as ignored for the current user
            var alreadyIgnored = await dbContext.IgnoredPlaylists
                .AnyAsync(p => p.PlaylistId == playlistId && p.UserId == userId, token);

            if (!alreadyIgnored)
            {
                dbContext.IgnoredPlaylists.Add(new IgnoredPlaylist
                {
                    PlaylistId = playlistId,
                    UserId = userId
                });

                await dbContext.SaveChangesAsync(token);
            }
        }
    }
}
