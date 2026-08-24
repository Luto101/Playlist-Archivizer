namespace PlaylistArchivizer.Application.Exceptions
{
    // For validation errors (400 Bad Request)
    public class ValidationException(string message) : Exception(message);
}
