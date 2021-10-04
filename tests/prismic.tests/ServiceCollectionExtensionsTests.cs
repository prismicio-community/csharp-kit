using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddPrismic_adds_required_dependencies()
        {
            var collection = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));


            collection.AddPrismic();

            var serviceProvider = collection.BuildServiceProvider();

            var cache = serviceProvider.GetService<ICache>();
            Assert.NotNull(cache);
            Assert.Equal(typeof(InMemoryCache), cache.GetType());

            var client = serviceProvider.GetService<PrismicHttpClient>();
            Assert.NotNull(client);
            Assert.Equal(typeof(PrismicHttpClient), client.GetType());

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            Assert.NotNull(httpContextAccessor);
            Assert.Equal(typeof(HttpContextAccessor), httpContextAccessor.GetType());

            var prismicApiAccessor = serviceProvider.GetService<IPrismicApiAccessor>();
            Assert.NotNull(prismicApiAccessor);
            Assert.Equal(typeof(DefaultPrismicApiAccessor), prismicApiAccessor.GetType());
        }

        [Fact]
        public void AddPrismic_with_custom_implementation_does_not_override_implementation()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<ICache, FakeCache>();
            collection.AddPrismic();
            var serviceProvider = collection.BuildServiceProvider();

            var cache = serviceProvider.GetService<ICache>();
            Assert.NotNull(cache);
            Assert.Equal(typeof(FakeCache), cache.GetType());

            cache = serviceProvider.GetService<FakeCache>();
            Assert.Null(cache);
        }

        [Fact]
        public void AddPrismic_with_HttpContextAwareness()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<ICache, FakeCache>();
            collection.AddPrismic();
            var serviceProvider = collection.BuildServiceProvider();

            var cache = serviceProvider.GetService<ICache>();
            Assert.NotNull(cache);
            Assert.Equal(typeof(FakeCache), cache.GetType());

            var accessor = serviceProvider.GetService<IPrismicApiAccessor>();
            Assert.NotNull(accessor);
            Assert.Equal(typeof(DefaultPrismicApiAccessor), accessor.GetType());
        }

        private class FakeCache : ICache
        {
            public JToken Get(string key)
            {
                throw new NotImplementedException();
            }

            public void Set(string key, long ttl, JToken item)
            {
                throw new NotImplementedException();
            }

            public Task<T> GetOrSetAsync<T>(string key, long ttl, Func<Task<T>> factory)
            {
                throw new NotImplementedException();
            }
        }
    }
}
