using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace PlaylistArchivizer.Infrastructure.Persistence.Converters
{
    public class StringListConverter() : ValueConverter<List<string>, string>(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>())
    { }
}
