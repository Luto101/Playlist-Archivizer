using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Helpers;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;
using System.Net.Http.Json;
using System.Text.Json;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Services
{
    public class SpotifySavedTrackService(IHttpClientFactory httpClientFactory) : ISpotifySavedTrackService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient("SpotifyClient");

        public async Task<IReadOnlyCollection<string>> GetUserSavedTrackIdsAsync(CancellationToken token = default)
        {
            List<string> savedTracksIds = [];

            // Spotify API allows fetching up to 50 saved tracks per request
            await PaginationHelper.ForEachRequestAsync(50, async parameters =>
            {
                // Requesting only the track IDs and total count to minimize bandwidth
                parameters["fields"] = "total, items.track.id";

                var savedTracksResponseRaw = await HttpHelper.GetAsync(_client, "me/tracks", parameters);

                var savedTracksResponse = await savedTracksResponseRaw.Content.ReadFromJsonAsync<GetSavedTrackIdsResponse>()
                    ?? throw new JsonException("Failed to deserialize Spotify get saved tracks ids response.");

                foreach (var item in savedTracksResponse.Items)
                    savedTracksIds.Add(item.Track.Id);

                return savedTracksResponse.Total;
            }, token);

            return savedTracksIds;
        }

        public async Task<IReadOnlyCollection<string>> GetSavedTrackIdsAsync(IEnumerable<string> trackIds,
                                                                             CancellationToken token = default)
        {
            List<string> savedTracksIds = [];

            foreach (var batch in trackIds.Chunk(40))
            {
                token.ThrowIfCancellationRequested();

                string idsParameter = string.Join(",", batch.Select(id => $"spotify:track:{id}"));

                Dictionary<string, string?> parameters = new()
                {
                    ["uris"] = idsParameter
                };

                var responseRaw = await HttpHelper.GetAsync(_client, "me/library/contains", parameters);

                var response = await responseRaw.Content.ReadFromJsonAsync<bool[]>(token)
                    ?? throw new JsonException("Failed to deserialize Spotify response.");

                // Check which tracks in the batch are saved and add their IDs to the result list
                for (int i = 0; i < response.Length; i++)
                {
                    if (response[i])
                        savedTracksIds.Add(batch[i]);
                }
            }

            return savedTracksIds;
        }

    }
}
