using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlaylistArchivizer.Application.Interfaces;

namespace PlaylistArchivizer.Infrastructure.Persistence.Converters
{
    /// <summary>A value converter that encrypts and decrypts string values using the provided IEncryptionService.</summary>
    public class EncryptionConverter(IEncryptionService encryptionService) : ValueConverter<string, string>(
            v => encryptionService.Encrypt(v),
            v => encryptionService.Decrypt(v)
            )
    { }
}
