using Microsoft.Extensions.Configuration;
using PlaylistArchivizer.UI.Core.Helpers;
using PlaylistArchivizer.UI.Core.Responses;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace PlaylistArchivizer.UI.Core
{
    public static class Connector
    {
        private static readonly string CLIENT_ID;
        private static readonly string CLIENT_SECRET;
        private static readonly string CALLBACK_URL;
        private const string SCOPES = "playlist-read-private user-library-read playlist-modify-public";

        // Date for token refreshing
        private static string? refreshToken;
        private static DateTime tokenExpirationDate;

        static Connector()
        {
            // CLIENT_ID and CLIENT_SECRET are in appsettings.json
            IConfigurationRoot config;
            config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

            CLIENT_ID = config["Api:ClientId"]!;
            CLIENT_SECRET = config["Api:ClientSecret"]!;
            CALLBACK_URL = config["Api:CallbackUrl"]!;

            if (string.IsNullOrWhiteSpace(CLIENT_ID) ||
                string.IsNullOrWhiteSpace(CLIENT_SECRET) ||
                string.IsNullOrWhiteSpace(CALLBACK_URL))
                throw new Exception("Incorrect appsettings.json file");
        }

        public async static Task ConnectAsync(HttpClient client)
        {
            string code = GetAccessCode();
            await GetAndSetTokenToHttpClientAsync(client, code);
        }

        public async static Task RefreshTokenAsync(HttpClient client)
        {
            // Refresh is unnecessary
            if (refreshToken == null || tokenExpirationDate > DateTime.UtcNow)
                return;

            SetIdAuthorization(client);

            // Http body
            Dictionary<string, string> body = new()
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken }
                };

            // Without it HttpHelper.Post will call Refresh again and it will start endless loop
            string oldToken = refreshToken;
            refreshToken = null;

            var response = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token",
                null, body, "application/x-www-form-urlencoded");

            // Sometimes old token is needed
            refreshToken = oldToken;

            await SetTokenAsync(client, response);
        }

        private static string GetAccessCode()
        {
            // Display Spotify page and get whether user grants permissions
            HttpListenerRequest result = DisplayUserAccessForm();
            var query = HttpUtility.ParseQueryString(result.Url!.Query);

            if (query["error"] != null)
                throw new Exception(query["error"]!);
            else
                return query["code"]!;
        }

        private async static Task GetAndSetTokenToHttpClientAsync(HttpClient client, string code)
        {
            SetIdAuthorization(client);

            // Http body
            Dictionary<string, string> body = new()
                {
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", CALLBACK_URL },
                    { "code", code }
                };

            var response = await HttpHelper.PostAsync(client, "https://accounts.spotify.com/api/token",
                null, body, "application/x-www-form-urlencoded");

            await SetTokenAsync(client, response);
        }

        private async static Task SetTokenAsync(HttpClient client, HttpResponseMessage response)
        {
            var jsonResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(await response.Content.ReadAsStreamAsync());

            // Token authorization
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jsonResponse!.access_token);

            // Spotify API sometimes doesn't return new token
            if (!string.IsNullOrWhiteSpace(jsonResponse.refresh_token))
                refreshToken = jsonResponse.refresh_token;

            tokenExpirationDate = DateTime.UtcNow.AddSeconds(jsonResponse.expires_in);
        }

        private static HttpListenerRequest DisplayUserAccessForm()
        {
            // Form query
            var parameters = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", CLIENT_ID },
                { "scope", SCOPES },
                { "redirect_uri", CALLBACK_URL }
            };

            string query = HttpHelper.FormQuery(parameters);

            // Open browser to get access
            Process p = new()
            {
                StartInfo = new()
                {
                    UseShellExecute = true,
                    FileName = "https://accounts.spotify.com/authorize" + query
                }
            };
            p.Start();

            // Listener to Spotify callback
            HttpListener listener = new();

            // Prefix URI must end with '/'
            listener.Prefixes.Add(CALLBACK_URL.TrimEnd('/') + "/");
            listener.Start();

            var task = listener.GetContextAsync();

            // 60 seconds time limit to lgoin to Spotify
            int index = Task.WaitAny(task, Task.Delay(60000));

            HttpListenerContext context;

            if (index == 0) // GetContextAsync task has ended first
                context = task.Result;
            else
                throw new TimeoutException("User did not complete login in 60s.");

            // Close tab
            string responseString = "<head><script>close();</script></head><body>Authorization completed. You can close this window. </body>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Send response
            using Stream output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            context.Response.ContentType = "text/html";

            context.Response.Close();

            return context.Request;
        }

        private static void SetIdAuthorization(HttpClient client)
        {
            // Id authorization
            byte[] bytes = Encoding.UTF8.GetBytes(CLIENT_ID + ":" + CLIENT_SECRET);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
        }
    }
}
