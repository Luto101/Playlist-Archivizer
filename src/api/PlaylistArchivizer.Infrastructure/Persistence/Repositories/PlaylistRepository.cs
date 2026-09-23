using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Domain.Models;
using PlaylistArchivizer.Infrastructure.Persistence.Data;
using PlaylistArchivizer.Infrastructure.Persistence.Mappers;

namespace PlaylistArchivizer.Infrastructure.Persistence.Repositories
{
    public class PlaylistRepository(ApplicationDbContext dbContext) : IPlaylistRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<List<Playlist>> GetArchivedPlaylistsAsync(string userId, CancellationToken token = default)
        {
            // Retrieve only the playlists belonging to the user
            var entities = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .Where(p => p.UserId == userId)
                .ToListAsync(token);

            return entities.Select(PlaylistMapper.Map).ToList();
        }

        public async Task AddPlaylistAsync(Playlist playlist, string userId, CancellationToken token = default)
        {
            var entity = PlaylistMapper.Map(playlist, userId);

            _dbContext.Playlists.Add(entity);
            await _dbContext.SaveChangesAsync(token);
        }

        public async Task UpdatePlaylistTracksAsync(string playlistId,
                                                    string snapshotId,
                                                    IEnumerable<Track> newTracks,
                                                    string userId,
                                                    CancellationToken token = default)
        {
            var playlistEntity = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token);

            if (playlistEntity == null)
                return;

            // Update the snapshot ID to reflect the latest state from Spotify
            playlistEntity.SnapshotId = snapshotId;

            // Append only newly discovered tracks to prevent duplicates within the archived playlist
            var existingTrackIds = playlistEntity.Tracks.Select(t => t.Id).ToHashSet();

            foreach (var track in newTracks)
            {
                if (!existingTrackIds.Contains(track.Id))
                {
                    playlistEntity.Tracks.Add(new TrackEntity
                    {
                        Id = track.Id,
                        PlaylistId = playlistId,
                        Name = track.Name,
                        AddedAt = track.AddedAt,
                        ImageUrl = track.ImageUrl,
                        Artists = track.Artists.ToList()
                    });
                }
            }

            await _dbContext.SaveChangesAsync(token);
        }

        public async Task RemovePlaylistAsync(string playlistId, string userId, CancellationToken token = default)
        {
            var playlist = await _dbContext.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token);

            if (playlist != null)
            {
                _dbContext.Playlists.Remove(playlist);
                await _dbContext.SaveChangesAsync(token);
            }
        }

        public async Task RemoveTrackAsync(string playlistId, string trackId, string userId, CancellationToken token = default)
        {
            // Ensure the track belongs to the playlist, and that playlist belongs to the user
            var track = await _dbContext.Tracks
                .FirstOrDefaultAsync(t => t.Id == trackId &&
                                     t.PlaylistId == playlistId &&
                                     _dbContext.Playlists.Any(p => p.Id == playlistId && p.UserId == userId), token);

            if (track != null)
            {
                _dbContext.Tracks.Remove(track);
                await _dbContext.SaveChangesAsync(token);
            }
        }
    }
}
