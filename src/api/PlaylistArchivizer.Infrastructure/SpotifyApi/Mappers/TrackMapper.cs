using PlaylistArchivizer.Domain.Models;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses.Schemas;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Mappers
{
    public static class TrackMapper
    {
        public static Track Map(SpotifyTrack track)
        {
            List<string> artists = [];

            // Add all artists to list
            foreach (var artist in track.Artists)
                artists.Add(artist.Name);

            string biggestImageURL = track.Album.Images.OrderByDescending(x => x.Width).First().Url;

            return new(track.Id, track.Name, null, artists, biggestImageURL);
        }
        public static Track Map(PlaylistItemsResponse.Item item)
        {
            Track track = Map(item.Track);

            DateTime? addedAt = null;

            // Correct time zone. TODO: Timezone is taken from Spotify profile
            if (item.AddedAt != null)
            {
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                addedAt = item.AddedAt.Value + localZone.BaseUtcOffset;
            }

            track.AddedAt = addedAt;

            return track;
        }
    }
}
