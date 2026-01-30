using PlaylistArchivizer.UI.Core.Models;
using PlaylistArchivizer.UI.Core.Responses;

namespace PlaylistArchivizer.UI.Core.Mappers
{
    public static class TrackMapper
    {
        public static Track Map(Responses.Schemas.Track track)
        {
            List<string> artists = [];

            // Add all artists to list
            foreach (var artist in track.artists)
                artists.Add(artist.name);

            string biggestImageURL = track.album.images.OrderByDescending(x => x.width).First().url;

            return new(track.id, track.name, null, artists, biggestImageURL);
        }
        public static Track Map(PlaylistItemsResponse.Item item)
        {
            Track track = Map(item.track);

            DateTime? addedAt = null;

            // Correct time zone. TODO: Timezone is taken from Spotify profile
            if (item.added_at != null)
            {
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                addedAt = item.added_at.Value + localZone.BaseUtcOffset;
            }

            track.AddedAt = addedAt;

            return track;
        }
    }
}
