using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Microsoft.AspNetCore.Http;

namespace prismic
{
    public class DefaultPrismicApiAccessor : IPrismicApiAccessor
    {

        readonly PrismicHttpClient _prismicHttpClient;
        readonly ILogger<Api> _logger;
        readonly ICache _cache;
        readonly PrismicSettings _settings;
        readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultPrismicApiAccessor(PrismicHttpClient prismicHttpClient, ILogger<Api> logger, ICache cache, IHttpContextAccessor httpContextAccessor)
        {
            _prismicHttpClient = prismicHttpClient;
            _logger = logger;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public DefaultPrismicApiAccessor(PrismicHttpClient prismicHttpClient, ILogger<Api> logger, ICache cache, IHttpContextAccessor httpContextAccessor, IOptions<PrismicSettings> settings)
            : this(prismicHttpClient, logger, cache, httpContextAccessor)
        {
            _settings = settings.Value;
        }

        /**
         * Entry point to get an {@link Api} object from settings
         */
        public Task<Api> GetApi()
        {
            if (_settings == null)
                throw new ArgumentNullException(nameof(_settings), "Settings must not be null");

            return GetApi(_settings.Endpoint, _settings.AccessToken);
        }

        /**
		 * Entry point to get an {@link Api} object.
		 * Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api");</code>
		 *
		 * @param url the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		 * @return the usable API object
		 */
        public Task<Api> GetApi(string endpoint)
            => GetApi(endpoint, null);

        /**
		 * Entry point to get an {@link Api} object.
		 * Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api", null, new Cache.BuiltInCache(999), new Logger.PrintlnLogger());</code>
		 *
		 * @param endpoint the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		 * @param accessToken Your Oauth access token if you wish to use one (to access future content releases, for instance)
		 */
        public async Task<Api> GetApi(string endpoint, string accessToken)
        {
            Api api = GetCachedApi(endpoint, accessToken);

            if (api != null)
                return api;

            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Invalid endpoint uri", nameof(endpoint));

            var url = endpoint;

            if (!string.IsNullOrWhiteSpace(accessToken))
                url += $"?access_token={WebUtility.UrlEncode(accessToken)}";

            JToken json = await _prismicHttpClient.Fetch(url);
            ApiData apiData = ApiData.Parse(json);

            api = new Api(apiData, _prismicHttpClient, _httpContextAccessor);

            SetCachedApi(endpoint, accessToken, api);

            return api;
        }

        private void SetCachedApi(string endpoint, string accessToken, Api api)
        {
            var items = _httpContextAccessor?.HttpContext?.Items;

            if (items == null || api == null)
                return;

            items[GetCacheKey(endpoint, accessToken)] = api;
        }

        private Api GetCachedApi(string endpoint, string accessToken)
        {
            var items = _httpContextAccessor?.HttpContext?.Items;

            if (items == null)
                return null;

            var key = GetCacheKey(endpoint, accessToken);

            if (!items.ContainsKey(key))
                return null;

            return (Api)items[key];
        }

        private string GetCacheKey(string endpoint, string accessToken) => $"PRISMIC::API__{endpoint}_{accessToken}";
    }
}
