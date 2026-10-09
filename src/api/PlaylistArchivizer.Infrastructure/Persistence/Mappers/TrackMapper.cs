using PlaylistArchivizer.Domain.Entities;
using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.Infrastructure.Persistence.Mappers
{
    public static class TrackMapper
    {
        public static Track Map(TrackEntity entity)
        {
            return new Track(entity.Id, entity.Name, entity.AddedAt, entity.Artists, entity.ImageUrl);
        }
    }
}
