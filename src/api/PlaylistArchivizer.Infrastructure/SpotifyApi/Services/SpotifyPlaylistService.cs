using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Models;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Helpers;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Mappers;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;
using System.Net.Http.Json;
using System.Text.Json;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Services
{
    public class SpotifyPlaylistService(IHttpClientFactory httpClientFactory) : ISpotifyPlaylistService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("SpotifyClient");

        public async Task<IEnumerable<Playlist>> GetPlaylistsAsync(CancellationToken token = default)
        {
            List<Playlist> playlists = [];

            // Spotify API allows fetching up to 50 playlists per request
            await PaginationHelper.ForEachRequestAsync(50, async parameters =>
            {
                // Requesting only specific fields to optimize network payload size
                parameters["fields"] = "total, items(id, images, name, snapshot_id)";

                var playlistsResponseRaw = await HttpHelper.GetAsync(_client, "me/playlists", parameters);

                var playlistsResponse = await playlistsResponseRaw.Content.ReadFromJsonAsync<PlaylistsResponse>()
                    ?? throw new JsonException("Failed to deserialize Spotify playlists response.");

                foreach (var item in playlistsResponse.Items)
                    playlists.AddRange(PlaylistMapper.Map(item));

                return playlistsResponse.Total;
            }, token);

            return playlists;
        }

        public async Task<Playlist> CreatePlaylistAsync(string name, CancellationToken token = default)
        {
            Dictionary<string, string> body = new()
            {
                ["name"] = name,
                ["description"] = "Playlist created by Playlist Archivizer"
            };

            Dictionary<string, string?> parameters = new()
            {
                ["fields"] = "id, images, name, snapshot_id"
            };

            var playlistResponseRaw = await HttpHelper.PostAsync(_client, "me/playlists", parameters, body, "application/json");

            var playlistResponse = await playlistResponseRaw.Content.ReadFromJsonAsync<PlaylistResponse>(token)
                ?? throw new JsonException("Failed to deserialize Spotify playlist response.");

            Playlist playlist = PlaylistMapper.Map(playlistResponse);

            return playlist;
        }

        public async Task AddTracksToPlaylistAsync(Playlist playlist, IEnumerable<Track> tracks, CancellationToken token = default)
        {
            // Process tracks chronologicaly based on when they were added to preserve their original order
            Queue<Track> trackQueue = new(tracks.OrderBy(x => x.AddedAt));
            string url = $"playlists/{playlist.Id}/tracks";

            while (trackQueue.Count > 0)
            {
                if (token.IsCancellationRequested)
                    break;

                List<string> tracksUris = [];

                // Spotify's Add Tracks to Playlist endpoint accepts a maximum of 100 tracks per batch
                for (int i = 0; i < 100 && trackQueue.Count > 0; i++)
                {
                    Track track = trackQueue.Dequeue();
                    tracksUris.Add($"spotify:track:{track.Id}");
                }

                string urisQueryString = string.Join(",", tracksUris);

                Dictionary<string, string> parameters = new()
                {
                    ["uris"] = urisQueryString
                };

                await HttpHelper.PostAsync(_client, url, null, parameters, "application/json");
            }
        }
    }
}