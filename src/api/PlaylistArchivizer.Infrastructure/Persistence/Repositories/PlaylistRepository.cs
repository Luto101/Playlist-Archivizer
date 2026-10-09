using Microsoft.EntityFrameworkCore;
using PlaylistArchivizer.Application.Exceptions;
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

            // Remove duplicate tracks from the playlist before adding it to the database
            var uniqueTracksInPlaylist = entity.Tracks
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .ToList();

            entity.Tracks = uniqueTracksInPlaylist;

            var trackIdsToFetch = entity.Tracks.Select(t => t.Id).ToHashSet();

            var globallyExistingTracks = await _dbContext.Tracks
                .Where(t => trackIdsToFetch.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, token);

            var finalTracks = new List<TrackEntity>();

            foreach (var track in entity.Tracks)
            {
                // Check if the track already exists globally
                if (globallyExistingTracks.TryGetValue(track.Id, out var existingGlobalTrack))
                    finalTracks.Add(existingGlobalTrack);
                else
                    finalTracks.Add(track);
            }

            entity.Tracks = finalTracks;

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
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token)
                ?? throw new NotFoundException("Playlist or UserId", playlistId + " or " + userId);

            playlistEntity.SnapshotId = snapshotId;

            var existingPlaylistTrackIds = playlistEntity.Tracks.Select(t => t.Id).ToHashSet();

            var tracksToProcess = newTracks.Where(t => !existingPlaylistTrackIds.Contains(t.Id)).ToList();

            // No new tracks to process
            if (tracksToProcess.Count == 0)
            {
                await _dbContext.SaveChangesAsync(token);
                return;
            }

            var trackIdsToFetch = tracksToProcess.Select(t => t.Id).ToHashSet();

            var globallyExistingTracks = await _dbContext.Tracks
                .Where(t => trackIdsToFetch.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, token);

            foreach (var track in tracksToProcess)
            {
                // Check if the track already exists globally
                if (globallyExistingTracks.TryGetValue(track.Id, out var existingGlobalTrack))
                    playlistEntity.Tracks.Add(existingGlobalTrack);
                else
                {
                    var newTrackEntity = new TrackEntity
                    {
                        Id = track.Id,
                        Name = track.Name,
                        AddedAt = track.AddedAt,
                        ImageUrl = track.ImageUrl,
                        Artists = track.Artists.ToList()
                    };

                    playlistEntity.Tracks.Add(newTrackEntity);

                    globallyExistingTracks.Add(track.Id, newTrackEntity);
                }
            }

            await _dbContext.SaveChangesAsync(token);
        }

        public async Task RemovePlaylistAsync(string playlistId, string userId, CancellationToken token = default)
        {
            var playlist = await _dbContext.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token)
                ?? throw new NotFoundException("Playlist or UserId", playlistId + " or " + userId);

            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync(token);
        }

        public async Task RemoveTrackAsync(string playlistId, string trackId, string userId, CancellationToken token = default)
        {
            // Retrieve the playlist along with its tracks
            var playlist = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token)
                ?? throw new NotFoundException("Playlist or UserId", playlistId + " or " + userId);

            // Find the track to remove
            var track = playlist.Tracks.FirstOrDefault(t => t.Id == trackId)
                ?? throw new NotFoundException("Track", trackId);

            playlist.Tracks.Remove(track);

            await _dbContext.SaveChangesAsync(token);
        }

        public async Task<IReadOnlyCollection<Track>> GetTracksFromPlaylist(string playlistId, string userId, CancellationToken token = default)
        {
            var playlist = await _dbContext.Playlists
                .Include(p => p.Tracks)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token)
                ?? throw new NotFoundException("Playlist or UserId", playlistId + " or " + userId);

            return playlist.Tracks.Select(TrackMapper.Map).ToList();
        }

        public async Task<string> GetPlaylistName(string playlistId, string userId, CancellationToken token = default)
        {
            var playlist = await _dbContext.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId, token)
                ?? throw new NotFoundException("Playlist or UserId", playlistId + " or " + userId);

            return playlist.Name;
        }
    }
}
