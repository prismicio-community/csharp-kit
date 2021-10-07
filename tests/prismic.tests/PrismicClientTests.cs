using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Net.Http.Headers;

namespace prismic.AspNetCore.Tests
{
    public class PrismicClientTests
    {
        readonly ICache _cache;
        readonly ILogger<PrismicHttpClient> _logger;

        readonly Uri _uri = new Uri("https://fake.cdn.prismic.io/api");

        public PrismicClientTests()
        {
            var serviceProvider = TestHelper.GetServiceCollection()
                .AddHttpContextAccessor()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            _logger = factory.CreateLogger<PrismicHttpClient>();
            _cache = TestHelper.CreateInMemoryCache();
        }

        [Fact]
        public async Task Fetch_returns_unexpected_error_when_status_code_is_not_OK_or_Unauthorized()
        {
            var handlerMock = CreateMockMessageHandler(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            });

            var prismicClient = CreateClient(handlerMock.Object);

            await AssertThrowsPrismicClientException(PrismicFetch(prismicClient), PrismicClientException.ErrorCode.UNEXPECTED);
            VerifySendAsync(handlerMock, Times.Exactly(1), MethodAndRequestUriMatch());
        }

        [Fact]
        public async Task Fetch_returns_invalid_token_error()
        {
            var handlerMock = CreateMockMessageHandler(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\": \"Invalid access token\"}"),
            });

            var prismicClient = CreateClient(handlerMock.Object);

            await AssertThrowsPrismicClientException(PrismicFetch(prismicClient), PrismicClientException.ErrorCode.INVALID_TOKEN);
            VerifySendAsync(handlerMock, Times.Exactly(1), MethodAndRequestUriMatch());
        }

        [Fact]
        public async Task Fetch_returns_invalid_preview_error()
        {
            var handlerMock = CreateMockMessageHandler(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(
                    "{\"message\":\"Release https://fake-repo.prismic.io/previews/abc-123-xyz?websitePreviewId=nonsense not found\"}"
                ),
            });

            var prismicClient = CreateClient(handlerMock.Object);

            await AssertThrowsPrismicClientException(PrismicFetch(prismicClient), PrismicClientException.ErrorCode.INVALID_PREVIEW);
            VerifySendAsync(handlerMock, Times.Exactly(1), MethodAndRequestUriMatch());
        }


        [Fact]
        public async Task Fetch_returns_authorization_needed_error()
        {
            var handlerMock = CreateMockMessageHandler(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\": \"Nothing to see\"}"),
            });

            var prismicClient = CreateClient(handlerMock.Object);

            await AssertThrowsPrismicClientException(PrismicFetch(prismicClient), PrismicClientException.ErrorCode.AUTHORIZATION_NEEDED);
            VerifySendAsyncOnce(handlerMock);
        }

        [Fact]
        public async Task Fetch_returns_json_body()
        {
            var handlerMock = CreateMockMessageHandler(() => OkResponse);

            var prismicClient = CreateClient(handlerMock.Object);

            var response = await prismicClient.Fetch(_uri.ToString());

            AssertResponseIsValid(response);
            VerifySendAsyncOnce(handlerMock);
        }

        [Fact]
        public async Task Fetch_returns_cached_response()
        {
            var handlerMock = CreateMockMessageHandler(() => OkResponse);

            _cache.Set($"prismic_request::{_uri}", 400L, JToken.Parse(OkResponseJson));

            var prismicClient = CreateClient(handlerMock.Object);

            var response = await prismicClient.Fetch(_uri.ToString());

            AssertResponseIsValid(response);
            VerifySendAsync(handlerMock, Times.Never(), MethodAndRequestUriMatch());
        }

        [Fact]
        public async Task Fetch_does_not_cache_response_without_max_age_value()
        {
            var handlerMock = CreateMockMessageHandler(() => OkResponse);

            var prismicClient = CreateClient(handlerMock.Object);

            var response = await prismicClient.Fetch(_uri.ToString());

            AssertResponseIsValid(response);
            VerifySendAsyncOnce(handlerMock);
            var cachedResponse = GetCachedResponse();
            Assert.Null(cachedResponse);
        }

        [Fact]
        public async Task Fetch_caches_response_with_valid_max_age_value()
        {
            var handlerMock = CreateMockMessageHandler(() =>
            {
                var msg = OkResponse;
                msg.Headers.Add(HeaderNames.CacheControl, new List<string> { "max-age=5000" });
                return msg;
            });

            var prismicClient = CreateClient(handlerMock.Object);

            var response = await prismicClient.Fetch(_uri.ToString());
            AssertResponseIsValid(response);

            var cachedResponse = GetCachedResponse();
            Assert.NotNull(cachedResponse);

            VerifySendAsyncOnce(handlerMock);
        }

        [Fact]
        public async Task Fetch_does_not_cache_response_with_invalid_max_age_value()
        {
            var handlerMock = CreateMockMessageHandler(() =>
            {
                var msg = OkResponse;
                msg.Headers.Add(HeaderNames.CacheControl, new List<string> { "no-store" });
                return msg;
            });

            var prismicClient = CreateClient(handlerMock.Object);

            var response = await prismicClient.Fetch(_uri.ToString());
            AssertResponseIsValid(response);
            VerifySendAsyncOnce(handlerMock);

            var cachedResponse = GetCachedResponse();
            Assert.Null(cachedResponse);
        }

        private Mock<HttpMessageHandler> CreateMockMessageHandler(Func<HttpResponseMessage> valueFunction)
        {
            var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(valueFunction)
               .Verifiable();

            return mock;
        }

        private void VerifySendAsyncOnce(Mock<HttpMessageHandler> handler)
            => VerifySendAsync(handler, Times.Exactly(1), MethodAndRequestUriMatch());

        private void VerifySendAsync(Mock<HttpMessageHandler> handler, Times timesCalled, Expression<Func<HttpRequestMessage, bool>> match)
            => handler.Protected().Verify(
               "SendAsync",
               timesCalled,
               ItExpr.Is(match),
               ItExpr.IsAny<CancellationToken>()
            );

        private Expression<Func<HttpRequestMessage, bool>> MethodAndRequestUriMatch()
            => req => req.Method == HttpMethod.Get && req.RequestUri == _uri;

        private async Task AssertThrowsPrismicClientException(Func<Task> action, PrismicClientException.ErrorCode errorCode)
        {
            var result = await Assert.ThrowsAsync<PrismicClientException>(action);
            Assert.Equal(errorCode, result.Code);
        }

        private Func<Task> PrismicFetch(PrismicHttpClient prismicClient) => () => prismicClient.Fetch(_uri.ToString());

        private HttpResponseMessage OkResponse => new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(OkResponseJson),
        };

        private readonly string OkResponseJson = "{\"OkResponse\": true}";

        private JToken GetCachedResponse() => _cache.Get($"prismic_request::{_uri}");

        private PrismicHttpClient CreateClient(HttpMessageHandler httpMessageHandler)
            => TestHelper.CreatePrismicHttpClient(_cache, _logger, httpMessageHandler);

        private void AssertResponseIsValid(JToken response) => Assert.True((bool)response["OkResponse"]);
    }
}
