namespace PlaylistArchivizer.UI.Core.Responses
{
    public class TokenResponse
    {
        public string access_token { get; set; } = default!;
        public string token_type { get; set; } = default!;
        public int expires_in { get; set; }
        public string refresh_token { get; set; } = default!;
        public string scope { get; set; } = default!;
    }
}
