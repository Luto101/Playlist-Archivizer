using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PlaylistArchivizer.Application.Dtos;
using PlaylistArchivizer.Application.Exceptions;
using PlaylistArchivizer.Application.Interfaces;
using System.Security.Cryptography;

namespace PlaylistArchivizer.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration config,
                                IAuthService authService,
                                ISpotifyLoginService spotifyLoginService,
                                ITokenService tokenService,
                                IAuthCodeService authCodeService) : ControllerBase
    {
        // Stores whitelisted client application URIs for post-login redirection
        private readonly HashSet<string> _allowedRedirects =
            config.GetSection("Auth:AllowedClientRedirectUris").Get<string[]>()?
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        /// <summary>
        /// Endpoint to initiate the Spotify login process. 
        /// It generates a Spotify authorization URL and redirects the user to it.
        /// </summary>
        [HttpGet("spotify-url")]
        public IActionResult LoginToSpotify([FromQuery] string redirectUri)
        {
            // Validate that the target client redirect URI is whitelisted in configuration
            if (!_allowedRedirects.Contains(redirectUri))
                throw new ValidationException("Invalid client redirect URI");

            // CSRF protection
            string state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

            CookieOptions cookieOptions = new()
            {
                IsEssential = true,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            Response.Cookies.Append("spotify_auth_state", state, cookieOptions);
            Response.Cookies.Append("spotify_client_redirect", redirectUri, cookieOptions);

            // Construct the official Spotify authorization URL
            string spotifyUrl = spotifyLoginService.GenerateLoginUrl(state);

            return Redirect(spotifyUrl);
        }

        /// <summary>Endpoint is called by Spotify after the user authorizes the application</summary>
        [HttpGet("spotify")]
        public async Task<IActionResult> LoginWithSpotify([FromQuery(Name = "code")] string spotifyCode,
                                                          [FromQuery] string state,
                                                          [FromQuery] string? error)
        {
            // Spotify error handling
            if (!string.IsNullOrEmpty(error))
                throw new ExternalServiceException("Spotify", error);

            if (string.IsNullOrEmpty(spotifyCode))
                throw new ValidationException("Missing authorization code");

            if (!Request.Cookies.TryGetValue("spotify_auth_state", out string? cookieState))
                throw new SessionExpiredException("Session expired. Please try logging in again");

            // CSRF protection
            if (cookieState != state)
                throw new ValidationException("Invalid state parameter. CSRF protection triggered");

            // Validate that the target client redirect URI is still whitelisted in configuration
            if (!Request.Cookies.TryGetValue("spotify_client_redirect", out string? redirectUri) || !_allowedRedirects.Contains(redirectUri))
                throw new ValidationException("Invalid or missing client redirect URI");

            Response.Cookies.Delete("spotify_auth_state");
            Response.Cookies.Delete("spotify_client_redirect");

            // Exchange the authorization code for the Spotify user data
            SpotifyUserDataDto userData = await authService.ProcessSpotifyLoginAsync(spotifyCode);

            // Generate one-time authorization code
            string code = authCodeService.CreateCode(userData.UserId);

            string successUrl = QueryHelpers.AddQueryString(redirectUri, "code", code);

            return Redirect(successUrl);
        }

        /// <summary>Exchanges a one-time authorization code for an access token.</summary>
        [HttpPost("exchange")]
        public IActionResult ExchangeCode([FromBody] ExchangeCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ValidationException("Missing authorization code");

            if (!authCodeService.TryConsumeCode(request.Code, out string userId))
                throw new ValidationException("Invalid or expired authorization code");

            string token = tokenService.GenerateToken(userId);

            return Ok(new { accessToken = token });
        }
    }
}
