using Microsoft.AspNetCore.Http;
using PlaylistArchivizer.Application.Interfaces;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi
{
    public class SpotifyAuthorizationHandler(IAuthService authService, IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User context is missing.");

            string token = await authService.GetValidSpotifyTokenAsync(userId);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
