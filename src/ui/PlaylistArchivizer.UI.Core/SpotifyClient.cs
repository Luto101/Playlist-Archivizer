using PlaylistArchivizer.UI.Core.Helpers;
using PlaylistArchivizer.UI.Core.Mappers;
using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.UI.Core.Persistence;
using PlaylistArchivizer.UI.Core.Responses;
using System.Text.Json;

namespace PlaylistArchivizer.UI.Core
{
    public class SpotifyClient
    {
        private readonly HttpClient _client;
        private HashSet<string> ignoredPlaylistsIds;
        private bool AreSavedTracksUpdated; // Ensure that UpdateSavedTracksAsync will be called once

        public List<Playlist> Playlists { get; private set; } = [];

        public SpotifyClient()
        {
            _client = new();
            ignoredPlaylistsIds = [];
            AreSavedTracksUpdated = false;
        }

        public static async Task<SpotifyClient> CreateAsync()
        {
            var client = new SpotifyClient();

            await Connector.ConnectAsync(client._client); // Authorization token will assign here to HttpClient

            client.ignoredPlaylistsIds = await IgnoredPlaylistsStorage.LoadAsync() ?? [];

            client.Playlists = await client.GetPlaylistsAsync();

            await PlaylistsStorage.SaveAsync(client.Playlists);

            return client;
        }

        public async Task RemoveTrackAsync(Playlist playlist, Track track)
        {
            Playlists.First(x => x == playlist).Tracks.Remove(track);
            await PlaylistsStorage.SaveAsync(Playlists);
        }

        public async Task RemovePlaylistAsync(Playlist playlist)
        {
            Playlists.Remove(playlist);
            await PlaylistsStorage.SaveAsync(Playlists);
        }

        /// <summary>
        /// Updates the IsSaved property of the track.
        /// It can be called once
        /// </summary>
        public async Task UpdateSavedTracksAsync()
        {
            if (AreSavedTracksUpdated)
                return;

            await UpdateTrackSavedStateAsync();
            await PlaylistsStorage.SaveAsync(Playlists);

            AreSavedTracksUpdated = true;
        }

        private async Task UpdateTrackSavedStateAsync()
        {
            int limit = 50;

            List<string> tracksIdsToCheck = [];

            foreach (var playlist in Playlists)
            {
                int lastUncheckedTrackIndex = 0;

                for (int i = 0; i < playlist.Tracks.Count; i++)
                {
                    tracksIdsToCheck.Add(playlist.Tracks[i].Id);

                    // Send checking request
                    if (i + 1 - lastUncheckedTrackIndex == limit || i == playlist.Tracks.Count - 1)
                    {
                        Dictionary<string, string> parameters = new()
                        {
                            ["ids"] = string.Join(",", tracksIdsToCheck)
                        };

                        var response = await HttpHelper.GetAsync(_client, "https://api.spotify.com/v1/me/tracks/contains", parameters);

                        bool[] IsCheckedTracksSaved = JsonSerializer.Deserialize<bool[]>(await response.Content.ReadAsStreamAsync())!;

                        // Update all tracks
                        for (int j = 0; j < IsCheckedTracksSaved.Length; j++)
                            playlist.Tracks[lastUncheckedTrackIndex + j].IsSaved = IsCheckedTracksSaved[j];

                        tracksIdsToCheck.Clear();
                        lastUncheckedTrackIndex = i + 1;
                    }
                }
            }
        }

        private async Task<List<Playlist>> GetPlaylistsAsync()
        {
            List<Playlist>? playlists = await PlaylistsStorage.LoadAsync();

            playlists ??= []; // Init list when is null

            int limit = 50;

            await PaginationHelper.ForEachRequestAsync(limit, async (parameters) =>
            {
                parameters["fields"] = "total, items(id, images, name, snapshot_id)";

                var response = await HttpHelper.GetAsync(_client, "https://api.spotify.com/v1/me/playlists", parameters);

                var jsonResponse = JsonSerializer.Deserialize<PlaylistsResponse>(await response.Content.ReadAsStreamAsync())!;

                foreach (var item in jsonResponse.items)
                {
                    if(ignoredPlaylistsIds.Contains(item.id))
                        continue;

                    // Try to find an existing playlist with the same name
                    Playlist? existingPlaylist = playlists.LastOrDefault(x => x.Name == item.name);

                    if (existingPlaylist != null)
                    {
                        // Only update if the snapshot is new
                        if (existingPlaylist.SnapshotId != item.snapshot_id)
                        {
                            Playlist newPlaylist = PlaylistMapper.Map(item, await GetTracks(item.id));
                            HashSet<string> existingTrackIds = new(existingPlaylist.Tracks.Select(t => t.Id));

                            foreach (var track in newPlaylist.Tracks)
                            {
                                // Adds only if not already present
                                if (existingTrackIds.Add(track.Id))
                                    existingPlaylist.Tracks.Add(track);
                            }
                        }
                    }
                    else // No playlist with the same name exists, create new
                    {
                        Playlist newPlaylist = PlaylistMapper.Map(item, await GetTracks(item.id));
                        playlists.Add(newPlaylist);
                    }
                }

                return jsonResponse.total;
            });

            return playlists;
        }

        private async Task<List<Track>> GetTracks(string playlistId)
        {
            List<Track> tracks = [];
            string? url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks";

            int limit = 50;

            await PaginationHelper.ForEachRequestAsync(limit, async (parameters) =>
            {
                parameters["fields"] = "total, items(added_at, track(album.images, artists.name, id, name, type, is_local))";

                var response = await HttpHelper.GetAsync(_client, url, parameters);

                var jsonResponse = JsonSerializer.Deserialize<PlaylistItemsResponse>(await response.Content.ReadAsStreamAsync())!;
                foreach (PlaylistItemsResponse.Item item in jsonResponse.items)
                {
                    if (item.track.is_local) // Skip local track. TODO: don't skip local
                        continue;

                    tracks.Add(TrackMapper.Map(item));
                }

                return jsonResponse.total;

            });

            return tracks;
        }

        private async Task<Playlist> CreatePlaylistAsync(string name)
        {
            // Http body
            Dictionary<string, string> body = new()
            {
                { "name", name },
                { "description", "Playlist created by Playlist Archivizer" }
            };

            Dictionary<string, string> parameters = new()
            {
                ["fields"] = "id, images, name, snapshot_id"
            };

            var response = await HttpHelper.PostAsync(_client, "https://api.spotify.com/v1/me/playlists", parameters, body, "application/json");
            var jsonResponse = JsonSerializer.Deserialize<PlaylistResponse>(await response.Content.ReadAsStreamAsync())!;

            Playlist playlist = PlaylistMapper.Map(jsonResponse);

            ignoredPlaylistsIds.Add(playlist.Id);
            await IgnoredPlaylistsStorage.SaveAsync(ignoredPlaylistsIds);

            return playlist;
        }

        private async Task AddTracksToPlaylist(Playlist playlist, List<Track> tracks)
        {
            // Queue sorted by added date
            Queue<Track> trackStack = new(tracks.OrderBy(x => x.AddedAt));

            string url = $"https://api.spotify.com/v1/playlists/{playlist.Id}/tracks";

            while (trackStack.Count > 0)
            {
                List<string> tracksUris = [];

                for (int j = 0; j < 100 && trackStack.Count > 0; j++)
                {
                    Track track = trackStack.Dequeue();

                    tracksUris.Add($"spotify:track:{track.Id}");
                }

                // Http body. Serializing is inside of PostAsync
                var payloadBody = new { uris = tracksUris };

                await HttpHelper.PostAsync(_client, url, null, payloadBody, "application/json");
            }
        }

        public async Task CeatePlaylistWithTracksAsync(string name, List<Track> tracks)
        {
            Playlist playlist = await CreatePlaylistAsync(name);

            await AddTracksToPlaylist(playlist, tracks);
        }

        public async Task<List<string>> GetUserSavedTracksIdsAsync(CancellationToken token)
        {
            List<string> savedTracksId = [];

            await PaginationHelper.ForEachRequestAsync(50, async (parameters) =>
            {
                parameters["fields"] = "total, items.track.id";
                var response = await HttpHelper.GetAsync(_client, "https://api.spotify.com/v1/me/tracks", parameters);

                var jsonResponse = JsonSerializer.Deserialize<GetSavedTrackIdsResponse>(await response.Content.ReadAsStreamAsync())!;

                // Add all ids to list
                foreach (var item in jsonResponse.items)
                    savedTracksId.Add(item.track.id);

                return jsonResponse.total;
            }, token);

            return savedTracksId;
        }

        public async Task<List<Track>> GetTrackInfoFromIds(Queue<string> ids)
        {
            List<Track> resultTracks = [];

            while (ids.Count > 0)
            {
                List<string> tracksIds = [];

                for (int j = 0; j < 50 && ids.Count > 0; j++)
                {
                    string id = ids.Dequeue();

                    tracksIds.Add(id);
                }

                Dictionary<string, string> parameters = [];
                parameters["ids"] = string.Join(",", tracksIds);
                parameters["fields"] = "tracks(album.images, artists.name, id, name, type, is_local)";

                var response = await HttpHelper.GetAsync(_client, "https://api.spotify.com/v1/tracks", parameters);

                var jsonResponse = JsonSerializer.Deserialize<GetTracksResponse>(await response.Content.ReadAsStreamAsync())!;

                foreach (var track in jsonResponse.tracks)
                    resultTracks.Add(TrackMapper.Map(track));
            }

            return resultTracks;
        }
    }
}