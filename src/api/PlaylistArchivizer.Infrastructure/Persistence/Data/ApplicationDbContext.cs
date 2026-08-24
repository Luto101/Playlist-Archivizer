using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Domain.Entities;

namespace PlaylistArchivizer.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<SpotifyCredential> SpotifyCredentials { get; set; }
    }
}
