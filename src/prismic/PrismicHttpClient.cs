using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace prismic
{
    public class PrismicHttpClient
    {
        private readonly HttpClient _client;
        private readonly ICache _cache;
        private readonly ILogger<PrismicHttpClient> _logger;

        public PrismicHttpClient(HttpClient client, ICache cache, ILogger<PrismicHttpClient> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }

        public Task<JToken> Fetch(string url) => FetchImpl(url);

        private async Task<JToken> FetchImpl(string url)
        {
            var key = $"prismic_request::{url}";

            var cachedResponse = _cache.Get(key);

            if (cachedResponse != null)
                return cachedResponse;

            _logger.LogDebug("Fetching URL: {url}", url);

            using (var response = await _client.GetAsync(url))
            {
                var body = await response.Content.ReadAsStringAsync();
                string errorText;
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var json = JToken.Parse(body);
                        var ttl = GetMaxAgeInSeconds(response.Headers);

                        if (ttl > 0)
                            _cache.Set(key, ttl, json);

                        return json;

                    case HttpStatusCode.Unauthorized:
                        errorText = (string)JObject.Parse(body)["error"];

                        _logger.LogWarning("Unauthorised request {message}", errorText);

                        throw new PrismicClientException(
                            errorText == "Invalid access token"
                                ? PrismicClientException.ErrorCode.INVALID_TOKEN
                                : PrismicClientException.ErrorCode.AUTHORIZATION_NEEDED,
                            errorText
                        );

                    case HttpStatusCode.NotFound:
                        var jsonBody = JObject.Parse(body);
                        errorText = (string)jsonBody["message"] ?? (string)jsonBody["error"];

                        if (!string.IsNullOrWhiteSpace(errorText) &&
                            (Regex.IsMatch(errorText, @"^Release (?:.*) not found$")
                            || errorText == "This preview token has expired"))
                            throw new PrismicClientException(PrismicClientException.ErrorCode.INVALID_PREVIEW, errorText);

                        throw new PrismicClientException(PrismicClientException.ErrorCode.UNEXPECTED, body);

                    default:
                        throw new PrismicClientException(PrismicClientException.ErrorCode.UNEXPECTED, body);
                }
            }
        }

        private static readonly Regex maxAgeValueRegex = new Regex(@"max-age=(\d+)");

        private long GetMaxAgeInSeconds(HttpResponseHeaders headers)
        {
            if (!headers.TryGetValues(HeaderNames.CacheControl, out var headerValues))
                return 0;

            var matchResult = maxAgeValueRegex.Match(headerValues.FirstOrDefault());

            if (!matchResult.Success || matchResult.Groups.Count < 2)
                return 0;

            return long.Parse(matchResult.Groups[1].Value);
        }
    }
}
