using PlaylistArchivizer.API.Responses;
using System.Text.Json;

namespace PlaylistArchivizer.API.Helpers
{
    public static class HttpHelper
    {
        public async static Task<HttpResponseMessage> GetAsync(HttpClient client, string url, Dictionary<string, string>? parameters = null, object? body = null, string? contentType = null) =>
            await SendAsync(HttpMethod.Get, client, url, parameters, body, contentType);

        public async static Task<HttpResponseMessage> PostAsync(HttpClient client, string url, Dictionary<string, string>? parameters = null, object? body = null, string? contentType = null) =>
            await SendAsync(HttpMethod.Post, client, url, parameters, body, contentType);

        // Forms query string from parameters dictionary.
        public static string FormQuery(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            var encodedParts = parameters
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}");

            return "?" + string.Join("&", encodedParts);
        }

        private static HttpContent? FormContent(object? body, string? contentType)
        {
            if (body == null || string.IsNullOrEmpty(contentType))
                return null;

            HttpContent? content = contentType switch
            {
                "application/json" => JsonContent.Create(body),
                "application/x-www-form-urlencoded" => new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)body),
                _ => null
            };

            return content;
        }

        private static async Task<HttpResponseMessage> SendAsync(
            HttpMethod method, HttpClient client, string url, Dictionary<string, string>? parameters = null, object? body = null, string? contentType = null)
        {
            if (parameters != null)
                url += FormQuery(parameters);

            using HttpRequestMessage message = new(method, url)
            {
                Content = FormContent(body, contentType)
            };

            var response = await client.SendAsync(message);

            await ThrowExceptionIfRequired(response);

            return response;
        }

        private async static Task ThrowExceptionIfRequired(HttpResponseMessage response)
        {
            if (response.StatusCode is >= System.Net.HttpStatusCode.BadRequest)
                throw new BadHttpRequestException(
                    "Status: " + response.StatusCode.ToString() +
                    ". " + await GetErrorMessageAsync(response.Content));
        }

        // Gets error message form response.
        private async static Task<string> GetErrorMessageAsync(HttpContent content)
        {
            if (content == null)
                return "No error content provided.";

            string rawString = await content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(rawString))
                return "Empty error response.";

            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawString);

                if (errorResponse?.error?.message != null)
                    return errorResponse.error.message;
                else
                    return rawString;
            }
            catch (JsonException)
            {
                return "Json parsing error.";
            }
        }
    }
}
