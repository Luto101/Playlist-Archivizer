using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Infrastructure.Persistence.Converters;

namespace PlaylistArchivizer.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<SpotifyCredential> SpotifyCredentials { get; set; }

        private readonly IEncryptionService _encryptionService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                    IEncryptionService encryptionService) : base(options)
        {
            _encryptionService = encryptionService;
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            EncryptionConverter encryptionConverter = new(_encryptionService);

            modelBuilder.Entity<SpotifyCredential>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.AccessToken).HasConversion(encryptionConverter);

                entity.Property(e => e.RefreshToken).HasConversion(encryptionConverter);
            });
        }
    }
}
