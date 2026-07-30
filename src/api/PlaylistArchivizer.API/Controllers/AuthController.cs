using Microsoft.AspNetCore.Mvc;
using PlaylistArchivizer.API.Helpers;
using PlaylistArchivizer.API.Services;

namespace PlaylistArchivizer.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration _config, IAuthService authService) : ControllerBase
    {
        [HttpGet("spotify-url")]
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

        // Spotify will redirect to this endpoint after user login
        [HttpGet("spotify")]
        public async Task<IActionResult> LoginWithSpotify([FromQuery] string code, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
                return BadRequest($"Spotify error: {error}");

            string token = await authService.ProcessSpotifyLoginAsync(code);

            return Ok(new { token }); // Change to the close window HTML page
        }
    }
}