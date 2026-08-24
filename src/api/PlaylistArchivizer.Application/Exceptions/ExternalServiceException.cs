namespace PlaylistArchivizer.Application.Exceptions
{
    // For exceptions that occur when interacting with external services e.g. Spotify API
    public class ExternalServiceException(string service, string message)
        : Exception($"Error from {service}: {message}");
}
