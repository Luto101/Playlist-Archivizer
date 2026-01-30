using System.Text.Json;

namespace PlaylistArchivizer.UI.Core.Persistence
{
    public static class IgnoredPlaylistsStorage
    {
        public static readonly string PATH =
            PathProvider.GetPath("ignored_playlists.json");

        // Returns null when file doesn't exist
        public static async Task<HashSet<string>?> LoadAsync()
        {
            if (!File.Exists(PATH))
                return null;

            var bytes = await File.ReadAllBytesAsync(PATH);
            return JsonSerializer.Deserialize<HashSet<string>>(bytes);
        }

        public static async Task SaveAsync(HashSet<string> ignoredPlaylistIds)
        {
            string? dir = Path.GetDirectoryName(PATH);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using var stream = new FileStream(PATH, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, ignoredPlaylistIds);
        }
    }
}