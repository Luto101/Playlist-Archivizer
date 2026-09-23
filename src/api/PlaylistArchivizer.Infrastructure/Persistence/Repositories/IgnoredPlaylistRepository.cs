using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Infrastructure.Persistence.Data;
using System.Security.Claims;

namespace PlaylistArchivizer.Infrastructure.Persistence.Repositories
{
    public class IgnoredPlaylistRepository : IIgnoredPlaylistRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _userId;

        public IgnoredPlaylistRepository(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;

            // Extract the authenticated user's ID from the JWT claims context
            _userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            if (string.IsNullOrEmpty(_userId))
                throw new UnauthorizedAccessException("User context is missing.");
        }

        public async Task<HashSet<string>> GetIgnoredPlaylistIdsAsync(CancellationToken token)
        {
            // Fetch only the ignored playlist IDs that belong to the currently authenticated user
            var ids = await _dbContext.IgnoredPlaylists
                .Where(p => p.UserId == _userId)
                .Select(p => p.PlaylistId)
                .ToListAsync(token);

            return ids.ToHashSet();
        }

        public async Task AddToIgnoredAsync(string playlistId, CancellationToken token)
        {
            // Check if this playlist is already marked as ignored for the current user
            var alreadyIgnored = await _dbContext.IgnoredPlaylists
                .AnyAsync(p => p.PlaylistId == playlistId && p.UserId == _userId, token);

            if (!alreadyIgnored)
            {
                _dbContext.IgnoredPlaylists.Add(new IgnoredPlaylist
                {
                    PlaylistId = playlistId,
                    UserId = _userId
                });

                await _dbContext.SaveChangesAsync(token);
            }
        }
    }
}
