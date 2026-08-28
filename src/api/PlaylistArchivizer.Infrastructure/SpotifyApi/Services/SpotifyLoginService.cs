using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using PlaylistArchivizer.Application.Dtos;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Helpers;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Services
{
    public class SpotifyLoginService : ISpotifyLoginService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _redirectUri;
        private readonly string _scope;
        private readonly string _clientId;
        private readonly string _basicAuthHeaderValue;

        public SpotifyLoginService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;

            _redirectUri = config["Spotify:RedirectUri"]
                ?? throw new InvalidOperationException("Spotify:RedirectUri configuration is missing.");

            _scope = config["Spotify:Scope"]
                ?? throw new InvalidOperationException("Spotify:Scope configuration is missing.");

            _clientId = config["Spotify:ClientId"]
                ?? throw new InvalidOperationException("Spotify:ClientId configuration is missing.");

            string clientSecret = config["Spotify:ClientSecret"]
                ?? throw new InvalidOperationException("Spotify:ClientSecret configuration is missing.");

            // Generate the Basic Authentication header value required by Spotify OAuth
            _basicAuthHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{clientSecret}"));
        }

        public string GenerateLoginUrl(string state)
        {
            Dictionary<string, string?> parameters = new()
            {
                { "response_type", "code" },
                { "client_id", _clientId },
                { "scope", _scope },
                { "redirect_uri", _redirectUri },
                { "state", state }
            };

            string url = QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize", parameters);

            return url;
        }

        public async Task<SpotifyUserDataDto> AuthenticateAsync(string code)
        {
            HttpClient client = _httpClientFactory.CreateClient();

            // Prepare the payload for exchanging the authorization code for access tokens
            Dictionary<string, string> body = new()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUri }
            };

            Dictionary<string, string> headers = new()
            {
                { "Authorization", $"Basic {_basicAuthHeaderValue}" }
            };

            // Request initial access and refresh tokens
            var tokenResponseRaw = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token",
                                                              null, headers, body, "application/x-www-form-urlencoded");

            var tokenResponse = await tokenResponseRaw.Content.ReadFromJsonAsync<TokenResponse>()
                ?? throw new JsonException("Failed to deserialize Spotify token response.");

            // Prepare authorized headers using the newly acquired Bearer token
            Dictionary<string, string> userHeaders = new()
            {
                { "Authorization", $"Bearer {tokenResponse.AccessToken}" }
            };

            // Fetch the profile data of the authenticated Spotify user
            var userResponseRaw = await HttpHelper.GetAsync(client, "https://api.spotify.com/v1/me", null, userHeaders);

            var userResponse = await userResponseRaw.Content.ReadFromJsonAsync<UserResponse>()
                ?? throw new JsonException("Failed to deserialize Spotify user data response.");

            // Return new SpotifyUserData with relevant information
            return new()
            {
                UserId = userResponse.AccountId,
                Token = new()
                {
                    AccessToken = tokenResponse.AccessToken,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    RefreshToken = tokenResponse.RefreshToken
                }
            };
        }

        public async Task<SpotifyTokenDto> RefreshTokenAsync(string refreshToken)
        {
            HttpClient client = _httpClientFactory.CreateClient();

            // Prepare the payload for renewing an expired access token using a refresh token
            Dictionary<string, string> body = new()
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            Dictionary<string, string> headers = new()
            {
                { "Authorization", $"Basic {_basicAuthHeaderValue}" }
            };

            // Request a renewed access token
            var tokenResponseRaw = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token",
                                                              null, headers, body, "application/x-www-form-urlencoded");

            var tokenResponse = await tokenResponseRaw.Content.ReadFromJsonAsync<TokenResponse>()
                ?? throw new JsonException("Failed to deserialize Spotify refresh token response.");

            return new()
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiresIn = tokenResponse.ExpiresIn,
                RefreshToken = tokenResponse.RefreshToken
            };
        }
    }
}
