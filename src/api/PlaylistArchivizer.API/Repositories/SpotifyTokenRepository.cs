using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.API.Data;
using PlaylistArchivizer.API.Entities;

namespace PlaylistArchivizer.API.Repositories
{
    public class SpotifyTokenRepository : ISpotifyTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public SpotifyTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SpotifyToken?> GetByUserIdAsync(string userId)
        {
            return await _context.SpotifyTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task UpsertAsync(SpotifyToken newToken)
        {
            var existingToken = await GetByUserIdAsync(newToken.UserId);

            if (existingToken != null)
            {
                existingToken.AccessToken = newToken.AccessToken;
                existingToken.RefreshToken = newToken.RefreshToken;
                existingToken.ExpiresIn = newToken.ExpiresIn;
                existingToken.CreatedAt = DateTime.UtcNow;

                _context.SpotifyTokens.Update(existingToken);
            }
            else
                await _context.SpotifyTokens.AddAsync(newToken);

            await _context.SaveChangesAsync();
        }

    }
}
