using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Xunit;
using prismic.Middleware;
using System;
using System.Collections.Generic;

namespace prismic.AspNetCore.Tests
{
    public class HtmlExtensionTests {
        [Fact]
        public void TestName()
        {
        //Given
        
        //When
        
        //Then
        }
    }
    public class PreviewExpiredExceptionHandlerTests
    {

        Task ThrowTokenException(HttpContext ctx) => throw new PrismicClientException(PrismicClientException.ErrorCode.INVALID_PREVIEW, "Token Expired");


        [Fact]
        public async Task MiddlewareHandlesExpiredTokenException()
        {
            //Given
            var request = new RequestDelegate(ThrowTokenException);
            var handler = new PreviewExpiredExceptionHandlerMiddleware(request);
            var httpContext = new DefaultHttpContext();

            await handler.InvokeAsync(httpContext);

            //When
            Assert.Contains(httpContext.Response.Headers, x => x.Key == "Set-Cookie");

        }

        public static IEnumerable<object[]> ExampleExceptions = new List<object[]> {
            new object[] {                  PrismicClientException.ErrorCode.AUTHORIZATION_NEEDED },
            new object[] {  PrismicClientException.ErrorCode.INVALID_TOKEN },
            new object[] {  PrismicClientException.ErrorCode.MALFORMED_URL },
            new object[] {  PrismicClientException.ErrorCode.UNEXPECTED }
        };

        [Theory]
        [MemberData(nameof(ExampleExceptions))]
        public async Task MiddlewareThrowsForOtherPrismicExceptions(PrismicClientException.ErrorCode errorCode)
        {
            //Given
            var request = new RequestDelegate(
                (HttpContext ctx) =>
                {
                    throw new PrismicClientException(errorCode, $"{errorCode}");
                }
            );

            var handler = new PreviewExpiredExceptionHandlerMiddleware(request);
            var httpContext = new DefaultHttpContext();

            Task action() => handler.InvokeAsync(httpContext);

            //When
            var excpetion = await Assert.ThrowsAsync<PrismicClientException>(action);
            Assert.Equal(errorCode, excpetion.Code);
        }


        [Fact]
        public async Task MiddlewareThrowsForOtherExceptions()
        {
            //Given
            var expectedExceptionMessage = "Test Exception";
            var request = new RequestDelegate(
                (HttpContext ctx) => throw new Exception(expectedExceptionMessage)
            );

            var handler = new PreviewExpiredExceptionHandlerMiddleware(request);
            var httpContext = new DefaultHttpContext();

            Task action() => handler.InvokeAsync(httpContext);

            //When
            var excpetion = await Assert.ThrowsAsync<Exception>(action);
            Assert.Equal(expectedExceptionMessage, excpetion.Message);
        }
    }
}
