using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.API.Entities;

namespace PlaylistArchivizer.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<SpotifyToken> SpotifyTokens { get; set; }
    }
}
