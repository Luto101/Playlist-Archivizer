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

        public async Task<IEnumerable<string>> GetUserSavedTracksIdsAsync(CancellationToken token = default)
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
    }
}
