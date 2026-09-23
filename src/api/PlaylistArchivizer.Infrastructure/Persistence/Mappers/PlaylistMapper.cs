using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Infrastructure.Persistence.Mappers
{
    public static class PlaylistMapper
    {
        public static Playlist Map(PlaylistEntity entity)
        {
            var tracks = entity.Tracks.Select(t => new Track(t.Id, t.Name, t.AddedAt, t.Artists, t.ImageUrl));
            return new Playlist(entity.Id, entity.Name, entity.SnapshotId, tracks);
        }

        public static PlaylistEntity Map(Playlist playlist, string userId)
        {
            return new PlaylistEntity
            {
                Id = playlist.Id,
                Name = playlist.Name,
                SnapshotId = playlist.SnapshotId,
                UserId = userId,
                Tracks = playlist.Tracks.Select(t => new TrackEntity
                {
                    Id = t.Id,
                    PlaylistId = playlist.Id,
                    Name = t.Name,
                    AddedAt = t.AddedAt,
                    ImageUrl = t.ImageUrl,
                    Artists = t.Artists.ToList()
                }).ToList()
            };
        }
    }
}
