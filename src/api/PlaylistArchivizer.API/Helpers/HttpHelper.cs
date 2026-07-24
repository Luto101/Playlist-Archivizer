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
                    ". " + await GetErrorMessageAsync(response));
        }

        // Gets error message form response.
        private async static Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            if (response.Content == null)
                return "No error content provided.";

            string rawString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(rawString))
                return $"Empty error response. Status: {response.StatusCode}";

            // Try multiple known shapes:
            // 1) Spotify token errors: { "error": "invalid_grant", "error_description": "..." }
            try
            {
                using var doc = JsonDocument.Parse(rawString);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("error_description", out var descProp))
                    {
                        string err = root.GetProperty("error").GetString() ?? "unknown_error";
                        return $"{err}: {descProp.GetString()} (raw: {rawString})";
                    }

                    // 2) other API error shape: { "error": { "message": "...", "status": 400 } }
                    if (root.TryGetProperty("error", out var errObj) && errObj.ValueKind == JsonValueKind.Object)
                    {
                        if (errObj.TryGetProperty("message", out var msgProp))
                        {
                            string message = msgProp.GetString() ?? rawString;
                            return $"Status: {response.StatusCode}. {message} (raw: {rawString})";
                        }
                    }

                    // 3) generic error field
                    if (root.TryGetProperty("error", out var errVal) && errVal.ValueKind == JsonValueKind.String)
                    {
                        return $"Error: {errVal.GetString()} (raw: {rawString})";
                    }
                }

                // Fallback to raw string if JSON but unknown shape
                return $"Unknown JSON error shape. Raw: {rawString}";
            }
            catch (JsonException)
            {
                // Not JSON or parsing failed; return raw body
                return $"Non-JSON error body: {rawString}";
            }
        }
    }
}