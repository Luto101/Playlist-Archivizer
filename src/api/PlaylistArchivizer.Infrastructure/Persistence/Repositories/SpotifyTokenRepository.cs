using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Infrastructure.Persistence.Data;

namespace PlaylistArchivizer.Infrastructure.Persistence.Repositories
{
    public class SpotifyTokenRepository(ApplicationDbContext context) : ISpotifyTokenRepository
    {
        public async Task<SpotifyCredential?> GetByUserIdAsync(string userId) =>
            await context.SpotifyCredentials.FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task UpsertAsync(SpotifyCredential newCredential)
        {
            var existingCredential = await GetByUserIdAsync(newCredential.UserId);

            if (existingCredential != null)
            {
                existingCredential.AccessToken = newCredential.AccessToken;
                existingCredential.RefreshToken = newCredential.RefreshToken;
                existingCredential.ExpiresIn = newCredential.ExpiresIn;
                existingCredential.CreatedAt = newCredential.CreatedAt;
            }
            else
                await context.SpotifyCredentials.AddAsync(newCredential);

            await context.SaveChangesAsync();
        }
    }
}
