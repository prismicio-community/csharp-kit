using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace prismic.AspNetCore.Tests
{
    public class InMemoryCacheTests
    {
        readonly InMemoryCache _cache;
        public InMemoryCacheTests()
        {
            _cache = TestHelper.CreateInMemoryCache();
        }

        [Fact]
        public void Get_returns_null_if_key_IsNullOrWhitespace()
        {
            Assert.Null(_cache.Get(null));
            Assert.Null(_cache.Get(string.Empty));
        }

        [Fact]
        public void Get_returns_null_if_entry_does_not_exist()
        {
            Assert.Null(_cache.Get("not_in_cache"));
        }

        [Fact]
        public void Get_returns_entry_if_it_exists()
        {
            var key = "in_cache";
            var token = JToken.Parse("{\"test\":true}");
            _cache.Set(key, 5000L, token);

            var result = _cache.Get(key);

            Assert.NotNull(result);
            Assert.True(result.Value<bool>("test"));
        }


        [Fact]
        public async Task GetOrSetAsync_returns_null_if_key_IsNullOrWhitespace()
        {
            Assert.Null(await _cache.GetOrSetAsync(null, 10L, Factory));
            Assert.Null(await _cache.GetOrSetAsync(string.Empty, 10L, Factory));
        }

        [Fact]
        public async Task GetOrSetAsync_returns_set_entry_if_it_does_not_exist()
        {
            var result = await _cache.GetOrSetAsync("not_in_cache", 10L, Factory);
            Assert.Equal(JTokenType.Boolean, result.Type);
            Assert.True((bool)result.Root);
        }

        [Fact]
        public async Task GetOrSetAsync_returns_entry_if_it_exists()
        {
            var key = "in_cache";
            var token = JToken.Parse("{\"test\":true}");
            _cache.Set(key, 5000L, token);

            var result = await _cache.GetOrSetAsync<JToken>(key, 10L, () => throw new XunitException("This should not be called"));

            Assert.NotNull(result);
            Assert.True(result.Value<bool>("test"));
        }

        static Task<JToken> Factory() => Task.FromResult(JToken.Parse("true"));

    }
}