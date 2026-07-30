using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PlaylistArchivizer.API.Helpers;
using PlaylistArchivizer.API.Services;
using System.Security.Cryptography;

namespace PlaylistArchivizer.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration _config, IAuthService _authService) : ControllerBase
    {
        [HttpGet("spotify-url")]
        public IActionResult LoginToSpotify([FromQuery] string redirectUri)
        {
            // Generate state to prevent CSRF attacks.
            var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            Response.Cookies.Append("spotify_auth_state", state, cookieOptions);
            Response.Cookies.Append("spotify_client_redirect", redirectUri, cookieOptions);

            var parameters = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", _config["Spotify:ClientId"]! },
                { "scope", _config["Spotify:Scopes"]! },
                { "redirect_uri", _config["Spotify:RedirectUri"]! },
                { "state", state }
            };

            string query = HttpHelper.FormQuery(parameters);

            var spotifyUrl = "https://accounts.spotify.com/authorize" + query;

            return Redirect(spotifyUrl);
        }

        // Spotify will redirect to this endpoint after user login
        [HttpGet("spotify")]
        public async Task<IActionResult> LoginWithSpotify([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
                return BadRequest($"Spotify error: {error}");

            // CSRF protection
            if (!Request.Cookies.TryGetValue("spotify_auth_state", out var cookieState) || cookieState != state)
                return BadRequest("Invalid state parameter. CSRF protection triggered.");

            // Validate the client redirect URI against the allowed list
            var allowedRedirects = _config.GetSection("Auth:AllowedClientRedirectUris").Get<string[]>();
            if (!Request.Cookies.TryGetValue("spotify_client_redirect", out var redirectUri) ||
                allowedRedirects == null || !allowedRedirects.Contains(redirectUri))
                return BadRequest("Invalid client redirect URI.");

            Response.Cookies.Delete("spotify_auth_state");
            Response.Cookies.Delete("spotify_client_redirect");

            string token = await _authService.ProcessSpotifyLoginAsync(code);

            var successUrl = QueryHelpers.AddQueryString(redirectUri, "token", token);

            return Redirect(successUrl);
        }
    }
}