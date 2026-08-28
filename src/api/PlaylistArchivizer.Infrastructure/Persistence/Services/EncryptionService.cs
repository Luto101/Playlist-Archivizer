using Microsoft.AspNetCore.DataProtection;
using PlaylistArchivizer.Application.Interfaces;

namespace PlaylistArchivizer.Infrastructure.Persistence.Services
{
    public class EncryptionService(IDataProtectionProvider provider) : IEncryptionService
    {
        private readonly IDataProtector _protector = provider.CreateProtector("spotify-tokens-purpose");

        public string Encrypt(string plainText) => _protector.Protect(plainText);

        public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
    }
}
