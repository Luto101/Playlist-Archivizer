namespace PlaylistArchivizer.UI.Core.Models
{
    public class Track(string id, string name, DateTime? addedAt, List<string> artists, string imageUrl)
    {
        public string Id { get; set; } = id;
        public string Name { get; set; } = name;
        public DateTime? AddedAt { get; set; } = addedAt; // Some very old playlists may return null
        public List<string> Artists { get; set; } = artists;
        public string ImageUrl { get; set; } = imageUrl; // The widest available image 
        
        /// <summary>
        /// Requires update using UpdateSavedTracksAsync()
        /// </summary>
        public bool? IsSaved { get; set; } = null;
    }
}
