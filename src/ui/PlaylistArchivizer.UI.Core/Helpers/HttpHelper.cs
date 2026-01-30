using PlaylistArchivizer.UI.Core.Responses;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace PlaylistArchivizer.UI.Core.Helpers
{
    public static class HttpHelper
    {
        public async static Task<HttpResponseMessage> GetAsync(HttpClient client, string url, Dictionary<string, string>? parameters = null, object? body = null, string? contentType = null) =>
            await SendAsync(HttpMethod.Get, client, url, parameters, body, contentType);

        public async static Task<HttpResponseMessage> PostAsync(HttpClient client, string url, Dictionary<string, string>? parameters = null, object? body = null, string? contentType = null) =>
            await SendAsync(HttpMethod.Post, client, url, parameters, body, contentType);

        public static string FormQuery(Dictionary<string, string> parameters)
        {
            var parts = new List<string>();

            foreach (var item in parameters)
                parts.Add($"{HttpUtility.UrlEncode(item.Key)}={HttpUtility.UrlEncode(item.Value)}");

            return "?" + string.Join("&", parts);
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpMethod method, HttpClient client, string url, Dictionary<string, string>? parameters = null, object? body = null, string? contentType = null)
        {
            await Connector.RefreshTokenAsync(client); // Check if token is expired

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

        private static HttpContent? FormContent(object? body, string? contentType)
        {
            HttpContent? content = null;

            // Form content based on content type
            if (body != null && contentType != null)
            {
                content = contentType switch
                {
                    "application/x-www-form-urlencoded" => new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)body),
                    "application/json" => new StringContent(JsonSerializer.Serialize(body)),
                    _ => null
                };

                if (content != null)
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            return content;
        }

        private async static Task ThrowExceptionIfRequired(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new("Status Code: " + (int)response.StatusCode + ". " + await GetErrorMessageAsync(response.Content));
        }


        // Gets error message form response.
        private async static Task<string> GetErrorMessageAsync(HttpContent content)
        {
            try
            {
                // In Spotify API all error structure looks the same
                return JsonSerializer.Deserialize<ErrorResponse>(await content.ReadAsStreamAsync())!.error.message;
            }
            catch // But sometimes it isn't error directly from API
            {
                return await content.ReadAsStringAsync();
            }

        }
    }
}
