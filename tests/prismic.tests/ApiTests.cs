using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class ApiTests
    {
        readonly ApiData _apiData;
        readonly ICache _cache;
        readonly ILogger<PrismicHttpClient> _clientLogger;
        readonly Uri _fakeTokenUri;
        private readonly DocumentLinkResolver _linkResolver = DocumentLinkResolver.For(dl => "/fake-link");
        private readonly string _defaultLink = "/";
        private readonly string _fakeLink = "/fake-link";

        public ApiTests()
        {
            var rawApiData = Fixtures.Get("api_data.json");
            _apiData = ApiData.Parse(rawApiData);

            _cache = TestHelper.CreateInMemoryCache();
            var serviceProvider = TestHelper.GetServiceCollection().BuildServiceProvider();

            _clientLogger = serviceProvider.GetRequiredService<ILogger<PrismicHttpClient>>();
            _fakeTokenUri = new Uri("http://fake.preview.token.io");
        }

        [Fact]
        public async Task PreviewSession_returns_defaultUrl_when_mainDocument_is_not_populated()
        {
            var handler = new Mock<HttpMessageHandler>();
            SetupTokenRequestHandler(handler, () => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

            var client = TestHelper.CreatePrismicHttpClient(_cache, _clientLogger, handler.Object);
            var api = CreateApi(client);
            var url = await api.PreviewSession(_fakeTokenUri.ToString(), _linkResolver, _defaultLink);
            Assert.Equal(_defaultLink, url);
        }

        [Fact]
        public async Task PreviewSession_returns_resolved_link_when_main_document_is_populated()
        {
            var handler = new Mock<HttpMessageHandler>();
            SetupTokenRequestHandler(handler, () => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"mainDocument\":\"VQ_hV31Za5EAy02H\"}")
            });

            SetupRequestHandler(
                handler,
                req => req.RequestUri.Query.Contains("VQ_hV31Za5EAy02H"),
                () => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Fixtures.GetFileContents("response.json"))
                });

            var client = TestHelper.CreatePrismicHttpClient(_cache, _clientLogger, handler.Object);
            var api = CreateApi(client);

            var url = await api.PreviewSession(_fakeTokenUri.ToString(), _linkResolver, _defaultLink);
            Assert.Equal(_fakeLink, url);

        }


        [Fact]
        public async Task PreviewSession_returns_default_link_when_response_has_no_results()
        {
            var handler = new Mock<HttpMessageHandler>();
            SetupTokenRequestHandler(handler, () => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"mainDocument\":\"VQ_hV31Za5EAy02H\"}")
            });

            var response = JToken.Parse(Fixtures.GetFileContents("response.json"));

            response["results"] = "[]";

            SetupRequestHandler(
                handler,
                req => req.RequestUri.Query.Contains("VQ_hV31Za5EAy02H"),
                () => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(response.ToString())
                });

            var client = TestHelper.CreatePrismicHttpClient(_cache, _clientLogger, handler.Object);
            var api = CreateApi(client);

            var url = await api.PreviewSession(_fakeTokenUri.ToString(), _linkResolver, _defaultLink);
            Assert.Equal(_defaultLink, url);
        }

        [Fact]
        public void Api_exposes_ApiData_Tags()
        {
            var client = TestHelper.CreatePrismicHttpClient(_cache, _clientLogger);
            var api = CreateApi(client);
            Assert.Single(api.Tags);
            Assert.Equal("test-tag", api.Tags.FirstOrDefault());
        }

        [Fact]
        public void Api_exposes_ApiData_Types()
        {
            var client = TestHelper.CreatePrismicHttpClient(_cache, _clientLogger);
            var api = CreateApi(client);
            Assert.Single(api.Types);
            var firstType = api.Types.FirstOrDefault();

            Assert.Equal("test_document", firstType.Key);
            Assert.Equal("Test Document", firstType.Value);
        }

        [Fact]
        public void Api_exposes_ApiData_Bookmarks()
        {
            var client = TestHelper.CreatePrismicHttpClient(_cache, _clientLogger);
            var api = CreateApi(client);
            Assert.Single(api.Bookmarks);
            var firstType = api.Bookmarks.FirstOrDefault();

            Assert.Equal("test_bookmark", firstType.Key);
            Assert.Equal("Test Bookmark", firstType.Value);
        }

        private Api CreateApi(PrismicHttpClient client) => new Api(_apiData, client);

        private void SetupTokenRequestHandler(Mock<HttpMessageHandler> handler, Func<HttpResponseMessage> valueFunction)
        {
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri == _fakeTokenUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(valueFunction);
        }

        private void SetupRequestHandler(Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match, Func<HttpResponseMessage> valueFunction)
        {
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is(match),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(valueFunction);
        }
    }
}
