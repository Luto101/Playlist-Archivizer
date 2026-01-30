using PlaylistArchivizer.UI.Core.Models;

namespace PlaylistArchivizer.UI.Core.Persistence
{
    public static class ImageStorage
    {
        private static readonly HttpClient _client = new();

        /// <summary>
        /// Gets and downloads image path if required. 
        /// </summary>
        /// <returns>Path to the image</returns>
        public static async Task<string> GetImagePathAsync(Track track)
        {
            string path = PathProvider.GetPath(track.Id, "Images");

            if (!File.Exists(path))
            {
                if (string.IsNullOrWhiteSpace(track.ImageUrl))
                    throw new Exception("Image URL missing.");
                
                byte[] data = await _client.GetByteArrayAsync(track.ImageUrl);
                File.WriteAllBytes(path, data);
            }

            return path;
        }
    }
}
