using PlaylistArchivizer.API.Helpers;
using PlaylistArchivizer.API.Models;
using PlaylistArchivizer.API.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PlaylistArchivizer.API.Services
{
    public class SpotifyLoginService : ISpotifyLoginService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public SpotifyLoginService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<SpotifyUserData> AuthenticateAsync(string code)
        {
            var client = _httpClientFactory.CreateClient();

            // Basic Authentication with Client ID and Client Secret
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config["Spotify:ClientId"]}:{_config["Spotify:ClientSecret"]}"))
            );

            var body = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _config["Spotify:RedirectUri"]! }
            };

            var response = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token", null, body, "application/x-www-form-urlencoded");

            var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(await response.Content.ReadAsStreamAsync());

            // Set the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse!.access_token);

            response = await HttpHelper.GetAsync(client, "https://api.spotify.com/v1/me");

            var userResponse = await JsonSerializer.DeserializeAsync<UserResponse>(await response.Content.ReadAsStreamAsync());

            return new(userResponse!, tokenResponse!);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var client = _httpClientFactory.CreateClient();

            // Basic Authentication with Client ID and Client Secret
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config["Spotify:ClientId"]}:{_config["Spotify:ClientSecret"]}"))
            );

            var body = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var response = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token", null, body, "application/x-www-form-urlencoded");

            var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(await response.Content.ReadAsStreamAsync());

            return tokenResponse!;
        }

    }
}
