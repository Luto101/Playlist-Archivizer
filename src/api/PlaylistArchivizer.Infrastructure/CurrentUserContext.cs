using Microsoft.AspNetCore.Http;
using PlaylistArchivizer.Application.Interfaces;
using System.Security.Claims;

namespace PlaylistArchivizer.Infrastructure
{
    public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
