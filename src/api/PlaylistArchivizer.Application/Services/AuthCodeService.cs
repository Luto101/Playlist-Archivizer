using Microsoft.Extensions.Caching.Memory;
using PlaylistArchivizer.Application.Interfaces;
using System.Security.Cryptography;

namespace PlaylistArchivizer.Application.Services
{
    public class AuthCodeService(IMemoryCache cache) : IAuthCodeService
    {
        public string CreateCode(string userId)
        {
            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            cache.Set(GetKey(code),
                      userId,
                      TimeSpan.FromSeconds(60));

            return code;
        }

        public bool TryConsumeCode(string code, out string userId)
        {
            string key = GetKey(code);

            if (!cache.TryGetValue(key, out userId!))
            {
                userId = string.Empty;
                return false;
            }

            // One-time use
            cache.Remove(key);

            return true;
        }

        // Generates a cache key for the given code.
        private static string GetKey(string code) => $"spotify-auth-code:{code}";
    }
}