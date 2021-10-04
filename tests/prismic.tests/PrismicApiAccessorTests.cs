using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class PrismicApiAccessorTests
    {
        [Fact]
        public Task DefaultAccessor_throws_exception_when_no_settings_provider_and_GetApi_is_called_with_no_arguments()
        {
            var prismic = TestHelper.GetDefaultAccessor();

            return Assert.ThrowsAsync<ArgumentNullException>("_settings", () => prismic.GetApi());
        }

        [Fact]
        public Task DefaultAccessor_throws_exception_when_endpoint_in_settings_is_invalid_and_GetApi_is_called_with_no_arguments()
        {
            var prismic = TestHelper.GetDefaultAccessor(new PrismicSettings());

            return Assert.ThrowsAsync<ArgumentException>("endpoint", () => prismic.GetApi());
        }

        [Fact]
        public async Task DefaultAccessor_gets_Api_when_settings_are_valid_and_GetApi_is_called_with_no_arguments()
        {
            var prismic = TestHelper.GetDefaultAccessor(new PrismicSettings
            {
                Endpoint = TestHelper.Endpoint
            });
            var api = await prismic.GetApi();
            Assert.NotNull(api.Master);
        }

        [Fact]
        public Task DefaultAccessor_throws_exception_when_GetApi_is_called_with_invalid_endpoint()
        {
            var prismic = TestHelper.GetDefaultAccessor();
            return Assert.ThrowsAsync<ArgumentException>("endpoint", () => prismic.GetApi(string.Empty));
        }

        [Fact]
        public async Task DefaultAccessor_gets_Api_when_GetApi_is_called_with_valid_endpoint()
        {
            var prismic = TestHelper.GetDefaultAccessor();
            var api = await prismic.GetApi(TestHelper.Endpoint);
            Assert.NotNull(api.Master);
        }

        [Fact]
        public async Task DefaultAccessor_caches_GetApi_requests()
        {
            var mock = new Mock<IHttpContextAccessor>();
            var items  = new Dictionary<object, object>();
            mock.Setup((x) => x.HttpContext.Items)
                .Returns(items);

            var serviceProvider = TestHelper.GetServiceCollection()
                .AddSingleton(mock.Object)
                .BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<Api>();

            var cache = TestHelper.CreateInMemoryCache();
            var httpClient = new PrismicHttpClient(new HttpClient(), cache, factory.CreateLogger<PrismicHttpClient>());
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();


            var accessor = new DefaultPrismicApiAccessor(
                httpClient,
                logger,
                cache,
                httpContextAccessor
            );

            var api = await accessor.GetApi(TestHelper.Endpoint);
            
            var key = $"PRISMIC::API__{TestHelper.Endpoint}_";
            Assert.NotNull(api);
            Assert.Contains(key, items.Keys);
            Assert.Equal(api, (Api)items[key]);

            var api2 = await accessor.GetApi(TestHelper.Endpoint);

            Assert.Equal(api, api2);
        }


        [Fact]
        public async Task DefaultAccessor_does_not_cache_GetApi_requests_when_HttpContext_not_present()
        {
            var accessor = TestHelper.GetDefaultAccessor();

            var api = await accessor.GetApi(TestHelper.Endpoint);
            Assert.NotNull(api);
        
            var api2 = await accessor.GetApi(TestHelper.Endpoint);
            Assert.NotEqual(api, api2);
        }
    }
}
