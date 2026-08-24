namespace PlaylistArchivizer.Application.Services
{
    public interface ITokenService
    {
        string GenerateToken(string userId);
    }
}