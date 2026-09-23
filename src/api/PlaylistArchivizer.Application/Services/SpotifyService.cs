// Location: PlaylistArchivizer.Application.Services/SpotifyService.cs
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Services
{
    public class SpotifyService(ISpotifyPlaylistService playlistService,
                                ISpotifyTrackService trackService,
                                ISpotifySavedTrackService savedTrackService,
                                IPlaylistRepository playlistRepository,
                                IIgnoredPlaylistRepository ignoredPlaylistRepository,
                                ICurrentUserContext currentUserContext) : ISpotifyService
    {
        private string UserId => currentUserContext.UserId
            ?? throw new UnauthorizedAccessException("User context is missing or user is not authenticated.");

        public async Task<IEnumerable<Playlist>> GetPlaylistsAsync(CancellationToken token = default)
        {
            var currentUserId = UserId; // Safely retrieved and validated at runtime
            var playlists = await playlistService.GetPlaylistsAsync(token);

            var archivedPlaylists = await playlistRepository.GetArchivedPlaylistsAsync(currentUserId, token);
            var ignoredPlaylistsIds = await ignoredPlaylistRepository.GetIgnoredPlaylistIdsAsync(currentUserId, token);

            foreach (var playlist in playlists)
            {
                if (ignoredPlaylistsIds.Contains(playlist.Id))
                    continue;

                Playlist? archivedPlaylist = archivedPlaylists.FirstOrDefault(p => p.Id == playlist.Id);

                if (archivedPlaylist != null)
                {
                    // If the SnapshotId matches, the playlist contents haven't changed since the last archive
                    if (archivedPlaylist.SnapshotId != playlist.SnapshotId)
                    {
                        IEnumerable<Track> tracks = await trackService.GetPlaylistTracksAsync(playlist.Id, token);

                        playlist.Tracks.AddRange(tracks);

                        await playlistRepository.UpdatePlaylistTracksAsync(playlist.Id, playlist.SnapshotId, tracks, currentUserId, token);

                        var archivedTrackIds = archivedPlaylist.Tracks.Select(t => t.Id).ToHashSet();

                        // Sync the in-memory object instance for the current application flow return value
                        foreach (var track in playlist.Tracks)
                        {
                            if (archivedTrackIds.Add(track.Id))
                                archivedPlaylist.Tracks.Add(track);
                        }
                    }
                }
                else
                {
                    // If the playlist does not exist locally, fetch all tracks and archive it as a new entry
                    IEnumerable<Track> tracks = await trackService.GetPlaylistTracksAsync(playlist.Id, token);
                    playlist.Tracks.AddRange(tracks);

                    await playlistRepository.AddPlaylistAsync(playlist, currentUserId, token);
                    archivedPlaylists.Add(playlist);
                }
            }

            return archivedPlaylists;
        }

        public async Task RemoveTrackAsync(Playlist playlist, string trackId, CancellationToken token = default)
        {
            await playlistRepository.RemoveTrackAsync(playlist.Id, trackId, UserId, token);
        }

        public async Task RemovePlaylistAsync(string playlistId, CancellationToken token = default)
        {
            await playlistRepository.RemovePlaylistAsync(playlistId, UserId, token);
        }

        public async Task CreatePlaylistWithTracksAsync(string name, IEnumerable<Track> tracks, CancellationToken token = default)
        {
            var playlist = await playlistService.CreatePlaylistAsync(name, token);

            // Ignore the new playlist to prevent the archiver from syncing it back onto itself
            await ignoredPlaylistRepository.AddToIgnoredAsync(playlist.Id, UserId, token);
            await playlistService.AddTracksToPlaylistAsync(playlist, tracks, token);
        }

        public Task<IEnumerable<string>> GetUserSavedTracksIdsAsync(CancellationToken token = default)
        {
            _ = UserId; // Triggers validation check
            return savedTrackService.GetUserSavedTracksIdsAsync(token);
        }

        public Task<IEnumerable<Track>> GetTrackInfoFromIdsAsync(Queue<string> ids, CancellationToken token = default)
        {
            _ = UserId; // Triggers validation check
            return trackService.GetTrackInfoFromIdsAsync(ids, token);
        }
    }
}
