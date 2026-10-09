using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Domain.Models;

namespace PlaylistArchivizer.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/spotify")]
    public class SpotifyController(ISpotifyService spotifyService) : ControllerBase
    {
        [HttpGet("playlists")]
        public async Task<ActionResult<IReadOnlyCollection<Playlist>>> GetPlaylists(CancellationToken token)
        {
            var playlists = await spotifyService.GetPlaylistsAsync(token);
            return Ok(new { playlists });
        }

        [HttpPost("playlists/{playlistId}")]
        public async Task<IActionResult> CreatePlaylist(string playlistId, CancellationToken token)
        {
            await spotifyService.CreateArchivedPlaylistInSpotify(playlistId, token);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete("playlists/{playlistId}")]
        public async Task<IActionResult> RemovePlaylist(string playlistId, CancellationToken token)
        {
            await spotifyService.RemovePlaylistAsync(playlistId, token);
            return NoContent();
        }

        [HttpDelete("playlists/{playlistId}/tracks/{trackId}")]
        public async Task<IActionResult> RemoveTrack(string playlistId, string trackId, CancellationToken token)
        {
            await spotifyService.RemoveTrackAsync(playlistId, trackId, token);
            return NoContent();
        }

        [HttpGet("tracks/free")]
        public async Task<ActionResult<IReadOnlyCollection<Track>>> GetFreeTracks(CancellationToken token)
        {
            var freeTracks = await spotifyService.GetFreeTracksAsync(token);
            return Ok(new { freeTracks });
        }

        [HttpGet("tracks/saved-from-archived")]
        public async Task<ActionResult<IReadOnlyCollection<string>>> GetSavedTrackIdsFromPlaylists(CancellationToken token)
        {
            var savedTrackIds = await spotifyService.GetSavedTrackIdsFromPlaylistsAsync(token);

            return Ok(new { savedTrackIds });
        }
    }
}
