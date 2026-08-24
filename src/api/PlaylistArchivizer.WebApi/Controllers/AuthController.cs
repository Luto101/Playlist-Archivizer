using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PlaylistArchivizer.Application.Dtos;
using PlaylistArchivizer.Application.Exceptions;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Application.Services;
using System.Security.Cryptography;

namespace PlaylistArchivizer.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IConfiguration config,
                                IAuthService authService,
                                ISpotifyLoginService spotifyLoginService,
                                ITokenService tokenService) : ControllerBase
    {
        // Stores white-listed client application URIs for post-login redirection
        private readonly HashSet<string> _allowedRedirects =
            config.GetSection("Auth:AllowedClientRedirectUris").Get<string[]>()?
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        [HttpGet("spotify-url")]
        public IActionResult LoginToSpotify([FromQuery] string redirectUri)
        {
            // Validate that the target client redirect URI is explicitly whitelisted in configuration
            if (!_allowedRedirects.Contains(redirectUri))
                throw new ValidationException("Invalid client redirect URI");

            // CSRF protection
            var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

            var cookieOptions = new CookieOptions
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

        // This endpoint is called by Spotify after the user authorizes the application
        [HttpGet("spotify")]
        public async Task<IActionResult> LoginWithSpotify([FromQuery] string code,
                                                          [FromQuery] string state,
                                                          [FromQuery] string? error)
        {
            // Spotify error handling
            if (!string.IsNullOrEmpty(error))
                throw new ExternalServiceException("Spotify", error);

            if (string.IsNullOrEmpty(code))
                throw new ValidationException("Missing authorization code");

            // Verify that the cookie still exists
            if (!Request.Cookies.TryGetValue("spotify_auth_state", out var cookieState))
                throw new SessionExpiredException("Session expired. Please try logging in again");

            // CSRF protection
            if (cookieState != state)
                throw new ValidationException("Invalid state parameter. CSRF protection triggered");

            // Validate that the target client redirect URI is still whitelisted in configuration
            if (!Request.Cookies.TryGetValue("spotify_client_redirect", out var redirectUri) || !_allowedRedirects.Contains(redirectUri))
                throw new ValidationException("Invalid or missing client redirect URI");

            Response.Cookies.Delete("spotify_auth_state");
            Response.Cookies.Delete("spotify_client_redirect");

            // Exchange the authorization code for the Spotify user data
            SpotifyUserDataDto userData = await authService.ProcessSpotifyLoginAsync(code);

            // Generate an application token
            string token = tokenService.GenerateToken(userData.UserId);

            // Append the generated application token as a query parameter to the final client redirect URL
            string successUrl = QueryHelpers.AddQueryString(redirectUri, "token", token);

            return Redirect(successUrl);
        }
    }
}
