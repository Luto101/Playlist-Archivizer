using Microsoft.AspNetCore.Http;
using PlaylistArchivizer.Application.Interfaces;
using System.Security.Claims;

namespace PlaylistArchivizer.Infrastructure
{
    public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // Dynamicznie pobiera UserId przy każdym wywołaniu właściwości
        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
