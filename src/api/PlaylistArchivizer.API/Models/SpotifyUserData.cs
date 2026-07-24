using PlaylistArchivizer.API.Responses;

namespace PlaylistArchivizer.API.Models
{
    public class SpotifyUserData
    {
        public string UserId { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = default!;

        public SpotifyUserData(UserResponse userResponse, TokenResponse tokenResponse)
        {
            UserId = userResponse.account_id;
            AccessToken = tokenResponse.access_token;
            ExpiresIn = tokenResponse.expires_in;
            RefreshToken = tokenResponse.refresh_token;
        }
    }
}
