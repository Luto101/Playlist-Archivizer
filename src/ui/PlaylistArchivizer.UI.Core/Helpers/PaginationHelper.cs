namespace PlaylistArchivizer.UI.Core.Helpers
{
    public static class PaginationHelper
    {
        /// <summary>
        /// Executes repeated requests against a paginated API using the "limit" and "offset" query parameters.
        /// </summary>
        /// <param name="limit">The maximum number of items to request per call.</param>
        /// <param name="fetchRequest">
        /// Performs the request with provided query parameters ("limit" and "offset") 
        /// and returns the total number of items.
        /// </param>
        public static async Task ForEachRequestAsync(int limit, Func<Dictionary<string, string>, Task<int>> fetchRequest, CancellationToken token = default)
        {
            int offset = 0, total;

            // Query parameters
            Dictionary<string, string> parameters = new()
            {
                { "limit", limit.ToString() }
            };

            do
            {
                if (token.IsCancellationRequested)
                    return;

                parameters["offset"] = offset.ToString();
                total = await fetchRequest(parameters);
                offset += limit;
            }
            while (offset < total);
        }
    }
}
