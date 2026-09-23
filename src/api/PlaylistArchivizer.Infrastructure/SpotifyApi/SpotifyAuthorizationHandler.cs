using PlaylistArchivizer.Application.Interfaces;
using System.Net.Http.Headers;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi
{
    public class SpotifyAuthorizationHandler(IAuthService authService, ICurrentUserContext currentUserContext) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userId = currentUserContext.UserId;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User context is missing.");

            string token = await authService.GetValidSpotifyTokenAsync(userId);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
