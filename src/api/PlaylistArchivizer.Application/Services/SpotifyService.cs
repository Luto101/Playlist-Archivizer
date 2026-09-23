using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Application.Services
{
    public class SpotifyService(ISpotifyPlaylistService playlistService,
                                ISpotifyTrackService trackService,
                                ISpotifySavedTrackService savedTrackService,
                                IPlaylistRepository playlistRepository,
                                IIgnoredPlaylistRepository ignoredPlaylistRepository) : ISpotifyService
    {
        public async Task<IEnumerable<Playlist>> GetPlaylistsAsync(CancellationToken token = default)
        {
            var playlists = await playlistService.GetPlaylistsAsync(token);

            // Fetch existing archived and ignored playlists for the current user from the database
            var archivedPlaylists = await playlistRepository.GetArchivedPlaylistsAsync(token);
            var ignoredPlaylistsIds = await ignoredPlaylistRepository.GetIgnoredPlaylistIdsAsync(token);

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

                        // Forward the newly discovered tracks to the repository to safely update the database
                        await playlistRepository.UpdatePlaylistTracksAsync(playlist.Id, playlist.SnapshotId, tracks, token);

                        // Sync the in-memory object instance for the current application flow return value
                        var archivedTrackIds = archivedPlaylist.Tracks.Select(t => t.Id).ToHashSet();

                        foreach (var track in playlist.Tracks)
                        {
                            if (archivedTrackIds.Add(track.Id))
                                archivedPlaylist.Tracks.Add(track);
                        }
                    }
                }
                else
                {
                    // Archive the newly found playlist entirely
                    IEnumerable<Track> tracks = await trackService.GetPlaylistTracksAsync(playlist.Id, token);
                    playlist.Tracks.AddRange(tracks);

                    // Persist the new playlist state into the database
                    await playlistRepository.AddPlaylistAsync(playlist, token);
                    archivedPlaylists.Add(playlist);
                }
            }

            return archivedPlaylists;
        }

        public async Task RemoveTrackAsync(Playlist playlist, string trackId, CancellationToken token = default)
        {
            // Delegate track deletion and user context checking directly to the repository
            await playlistRepository.RemoveTrackAsync(playlist.Id, trackId, token);
        }

        public async Task RemovePlaylistAsync(string playlistId, CancellationToken token = default)
        {
            // Delegate playlist removal and user ownership validation directly to the repository
            await playlistRepository.RemovePlaylistAsync(playlistId, token);
        }

        public async Task CreatePlaylistWithTracksAsync(string name, IEnumerable<Track> tracks, CancellationToken token = default)
        {
            var playlist = await playlistService.CreatePlaylistAsync(name, token);

            // Automatically add the newly created playlist to the ignored list to prevent archiving it back onto itself
            await ignoredPlaylistRepository.AddToIgnoredAsync(playlist.Id, token);

            await playlistService.AddTracksToPlaylistAsync(playlist, tracks, token);
        }

        public Task<IEnumerable<string>> GetUserSavedTracksIdsAsync(CancellationToken token) =>
            savedTrackService.GetUserSavedTracksIdsAsync(token);

        public Task<IEnumerable<Track>> GetTrackInfoFromIdsAsync(Queue<string> ids, CancellationToken token = default) =>
             trackService.GetTrackInfoFromIdsAsync(ids, token);
    }
}
