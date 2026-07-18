using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistArchivizer.API.Helpers;
using PlaylistArchivizer.API.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PlaylistArchivizer.API.Controllers
{
    [ApiController]
    [Route("api/spotify")]
    public class SpotifyController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public SpotifyController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpGet("login")]
        public IActionResult LoginToSpotify()
        {
            var parameters = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", _config["Spotify:ClientId"]! },
                { "scope", _config["Spotify:Scopes"]! },
                { "redirect_uri", _config["Spotify:RedirectUri"]! }
            };

            string query = HttpHelper.FormQuery(parameters);

            var spotifyUrl = "https://accounts.spotify.com/authorize" + query;

            return Ok(new { url = spotifyUrl });
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
                return BadRequest($"Spotify error: {error}");

            var client = _httpClientFactory.CreateClient();

            var body = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _config["Spotify:RedirectUri"]! }
            };

            // Basic Authentication with Client ID and Client Secret
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config["Spotify:ClientId"]}:{_config["Spotify:ClientSecret"]}"))
            );

            var response = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token", null, body, "application/x-www-form-urlencoded");

            var jsonResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(await response.Content.ReadAsStreamAsync());

            // --- TUTAJ ROZPOCZYNA SIĘ TWOJA LOGIKA ---
            // W tym miejscu musisz przypisać 'accessToken' oraz 'refreshToken' 
            // do zalogowanego aktualnie użytkownika w Twojej bazie danych.
            // ------------------------------------------

            return Ok(jsonResponse!.access_token);
        }
    }
}
