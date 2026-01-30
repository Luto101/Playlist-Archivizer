namespace PlaylistArchivizer.UI.Core.Responses
{
    public class ErrorResponse
    {
        public Error error { get; set; } = default!;

        public class Error
        {
            public int status { get; set; }
            public string message { get; set; } = default!;
        }
    }
}
