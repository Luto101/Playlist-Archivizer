using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Domain.Models;
using PlaylistArchivizer.Infrastructure.Persistence.Data;
using PlaylistArchivizer.Infrastructure.Persistence.Mappers;
using System.Security.Claims;

namespace PlaylistArchivizer.Infrastructure.Persistence.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _userId;

        public PlaylistRepository(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;

            // Extract the authenticated user's ID from the JWT claims context
            _userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            if (string.IsNullOrEmpty(_userId))
                throw new UnauthorizedAccessException("User context is missing.");
        }

        public async Task<List<Playlist>> GetArchivedPlaylistsAsync(CancellationToken token)
        {
            // Retrieve only the playlists belonging to the currently authenticated user
            var entities = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .Where(p => p.UserId == _userId)
                .ToListAsync(token);

            return entities.Select(PlaylistMapper.Map).ToList();
        }

        public async Task AddPlaylistAsync(Playlist playlist, CancellationToken token)
        {
            var entity = PlaylistMapper.Map(playlist, _userId);

            _dbContext.Playlists.Add(entity);
            await _dbContext.SaveChangesAsync(token);
        }

        public async Task UpdatePlaylistTracksAsync(string playlistId, string snapshotId, IEnumerable<Track> newTracks, CancellationToken token)
        {
            var playlistEntity = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == _userId, token);

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

        public async Task RemovePlaylistAsync(string playlistId, CancellationToken token)
        {
            var playlist = await _dbContext.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == _userId, token);

            if (playlist != null)
            {
                _dbContext.Playlists.Remove(playlist);
                await _dbContext.SaveChangesAsync(token);
            }
        }

        public async Task RemoveTrackAsync(string playlistId, string trackId, CancellationToken token)
        {
            // Ensure the track belongs to the playlist, and that playlist belongs to the current user
            var track = await _dbContext.Tracks
                .FirstOrDefaultAsync(t => t.Id == trackId &&
                                          t.PlaylistId == playlistId &&
                                          _dbContext.Playlists.Any(p => p.Id == playlistId && p.UserId == _userId), token);

            if (track != null)
            {
                _dbContext.Tracks.Remove(track);
                await _dbContext.SaveChangesAsync(token);
            }
        }
    }
}
