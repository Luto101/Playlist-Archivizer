namespace PlaylistArchivizer.Domain.Models
{
    public class Track
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public DateTime? AddedAt { get; set; }
        public string ImageUrl { get; init; }
        public IReadOnlyCollection<string> Artists => _artists.AsReadOnly();

        private readonly List<string> _artists = [];

        public Track(string id, string name, DateTime? addedAt, IEnumerable<string> artists, string imageUrl)
        {
            Id = id;
            Name = name;
            AddedAt = addedAt;
            ImageUrl = imageUrl;
            _artists.AddRange(artists ?? []);
        }
    }
}
