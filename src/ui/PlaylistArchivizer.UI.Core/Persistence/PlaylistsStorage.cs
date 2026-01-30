using PlaylistArchivizer.UI.Core.Models;
using System.Text.Json;

namespace PlaylistArchivizer.UI.Core.Persistence
{
    public static class PlaylistsStorage
    {
        public static readonly string PATH = 
            PathProvider.GetPath("playlists.json");

        // Returns null when file doesn't exist
        public static async Task<List<Playlist>?> LoadAsync()
        {
            List<Playlist>? playlists = null;

            if (File.Exists(PATH))
                playlists = JsonSerializer.Deserialize<List<Playlist>>(await File.ReadAllBytesAsync(PATH));

            return playlists;
        }

        public async static Task SaveAsync(List<Playlist> playlists)
        {
            // Ensure directory exists
            string? dir = Path.GetDirectoryName(PATH);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using StreamWriter stream = new(PATH);

            await JsonSerializer.SerializeAsync(stream.BaseStream, playlists);
        }
    }
}
