namespace PlaylistArchivizer.Application.Interfaces
{
    public interface IAuthCodeService
    {
        /// <summary>Creates a one-time use code for the given user ID and saves it into cache with 60-second expiration.</summary>
        string CreateCode(string userId);
        /// <summary>Consumes the user ID associated with a one-time use code.</summary>
        /// <returns>If the code is invalid or expired, returns false. Otherwise, returns true and outputs the user ID.</returns>
        bool TryConsumeCode(string code, out string userId);
    }
}