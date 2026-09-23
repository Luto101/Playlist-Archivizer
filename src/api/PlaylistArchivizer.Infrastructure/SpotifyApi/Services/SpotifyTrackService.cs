using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Models;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Helpers;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Mappers;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;
using System.Net.Http.Json;
using System.Text.Json;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Services
{
    public class SpotifyTrackService(IHttpClientFactory httpClientFactory) : ISpotifyTrackService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("SpotifyClient");

        public async Task<IEnumerable<Track>> GetPlaylistTracksAsync(string playlistId, CancellationToken token = default)
        {
            List<Track> tracks = [];
            string url = $"playlists/{playlistId}/tracks";

            // Spotify API allows fetching up to 50 tracks per request
            await PaginationHelper.ForEachRequestAsync(50, async parameters =>
            {
                // Requesting only fields required to build the Track model to reduce payload size
                parameters["fields"] = "total, items(added_at, track(album.images, artists.name, id, name, type, is_local))";

                var playlistItemsResponseRaw = await HttpHelper.GetAsync(_client, url, parameters);

                var playlistItemsResponse = await playlistItemsResponseRaw.Content.ReadFromJsonAsync<PlaylistItemsResponse>(token)
                    ?? throw new JsonException("Failed to deserialize Spotify playlist items response.");

                foreach (var item in playlistItemsResponse.Items)
                {
                    // Local files uploaded by users lack required streaming metadata and URLs
                    // TODO: Remove it
                    if (item.Track.IsLocal)
                        continue;

                    tracks.Add(TrackMapper.Map(item));
                }

                return playlistItemsResponse.Total;
            }, token);

            return tracks;
        }

        public async Task<IEnumerable<Track>> GetTrackInfoFromIdsAsync(Queue<string> ids, CancellationToken token = default)
        {
            List<Track> resultTracks = [];

            while (ids.Count > 0)
            {
                Queue<string> tracksIds = [];

                // Spotify's Get Several Tracks endpoint accepts a maximum of 50 IDs per batch
                for (int i = 0; i < 50 && ids.Count > 0; i++)
                    tracksIds.Enqueue(ids.Dequeue());

                Dictionary<string, string?> parameters = new()
                {
                    ["ids"] = string.Join(",", tracksIds),
                    ["fields"] = "tracks(album.images, artists.name, id, name, type, is_local)"
                };

                var tracksResponseRaw = await HttpHelper.GetAsync(_client, "tracks", parameters);

                var tracksResponse = await tracksResponseRaw.Content.ReadFromJsonAsync<GetTracksResponse>(token)
                        ?? throw new JsonException("Failed to deserialize Spotify tracks response.");

                foreach (var track in tracksResponse.Tracks)
                    resultTracks.Add(TrackMapper.Map(track));
            }

            return resultTracks;
        }
    }
}
