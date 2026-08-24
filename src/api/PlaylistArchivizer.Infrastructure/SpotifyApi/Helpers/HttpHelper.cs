using Microsoft.AspNetCore.WebUtilities;
using PlaylistArchivizer.Application.Exceptions;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Responses;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PlaylistArchivizer.Infrastructure.SpotifyApi.Helpers
{
    public static class HttpHelper
    {
        public async static Task<HttpResponseMessage> GetAsync(HttpClient client,
                                                               string url,
                                                               Dictionary<string, string?>? parameters = null,
                                                               Dictionary<string, string>? headers = null,
                                                               object? body = null,
                                                               string? contentType = null)
        => await SendAsync(HttpMethod.Get, client, url, parameters, headers, body, contentType);

        public async static Task<HttpResponseMessage> PostAsync(HttpClient client,
                                                                string url,
                                                                Dictionary<string, string?>? parameters = null,
                                                                Dictionary<string, string>? headers = null,
                                                                object? body = null,
                                                                string? contentType = null)
        => await SendAsync(HttpMethod.Post, client, url, parameters, headers, body, contentType);

        // Configures the HTTP request body content based on the specified media type
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

        // Core method to build, configure, and execute the asynchronous HTTP request
        private static async Task<HttpResponseMessage> SendAsync(HttpMethod method,
                                                                 HttpClient client,
                                                                 string url,
                                                                 Dictionary<string, string?>? parameters = null,
                                                                 Dictionary<string, string>? headers = null,
                                                                 object? body = null,
                                                                 string? contentType = null)
        {
            // Appends query string parameters
            if (parameters != null)
                url = QueryHelpers.AddQueryString(url, parameters);

            using HttpRequestMessage message = new(method, url)
            {
                Content = FormContent(body, contentType)
            };

            // Appends HTTP headers
            if (headers != null)
                foreach (var header in headers)
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);

            var response = await client.SendAsync(message);

            // Validates the response status code before returning
            await ThrowExceptionIfRequired(response);

            return response;
        }

        // Throws an exception containing descriptive API details
        private async static Task ThrowExceptionIfRequired(HttpResponseMessage response)
        {
            // Throw ExternalServiceException 
            if (response.StatusCode is >= HttpStatusCode.BadRequest)
                throw new ExternalServiceException("Spotify", // Hardcoded service name for now, can be made dynamic if needed
                                                   $"{await GetErrorMessageAsync(response.Content)} Status: {response.StatusCode}");
        }

        // Gets error message form response
        private async static Task<string> GetErrorMessageAsync(HttpContent content)
        {
            if (content == null)
                return "No error content provided.";

            string rawString = await content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(rawString))
                return "Empty error response.";

            try
            {
                // Attempts to parse standard Spotify structured error payload
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawString);

                if (errorResponse?.Error?.Message != null)
                    return errorResponse.Error.Message;
                else
                    return rawString;
            }
            catch (JsonException)
            {
                return $"Json parsing error. Raw response: {rawString}";
            }
        }
    }
}
