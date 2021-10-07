using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace prismic.AspNetCore.Tests
{
    public static class TestHelper
    {
        public static readonly string Endpoint = "https://apsnet-core-sdk.cdn.prismic.io/api";
        public static DefaultPrismicApiAccessor GetDefaultAccessor(PrismicSettings settings = null)
        {
            var serviceProvider = GetServiceCollection().AddHttpContextAccessor().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<Api>();

            var cache = CreateInMemoryCache();
            var httpClient = new PrismicHttpClient(new HttpClient(), cache, factory.CreateLogger<PrismicHttpClient>());
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            if (settings == null)
                return new DefaultPrismicApiAccessor(
                    httpClient,
                    logger,
                    cache,
                    httpContextAccessor
                );

            return new DefaultPrismicApiAccessor(
                    httpClient,
                    logger,
                    cache,
                    httpContextAccessor,
                    Options.Create(settings)
                );
        }

        public static IServiceCollection GetServiceCollection()
            => new ServiceCollection().AddLogging();

        public static Task<Api> GetApi(string url)
        {
            var accessor = GetDefaultAccessor();

            return accessor.GetApi(url);
        }

        public static InMemoryCache CreateInMemoryCache()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            return new InMemoryCache(memoryCache);
        }

        public static PrismicHttpClient CreatePrismicHttpClient(ICache cache, ILogger<PrismicHttpClient> logger, HttpMessageHandler httpMessageHandler = null)
        {
            var httpClient = httpMessageHandler == null ? new HttpClient() : new HttpClient(httpMessageHandler);
            return new PrismicHttpClient(httpClient, cache, logger);
        }
    }
}
