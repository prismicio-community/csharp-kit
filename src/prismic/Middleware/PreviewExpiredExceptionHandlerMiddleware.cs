using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace prismic.Middleware
{
    /// <summary>
    /// Handle preview expiration exceptions removing the cookie and redirecting to original request.
    /// </summary>
    public class PreviewExpiredExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public PreviewExpiredExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (PrismicClientException ex) when (ex.Code == PrismicClientException.ErrorCode.INVALID_PREVIEW)
            {
                context.Response.Cookies.Delete(Api.PREVIEW_COOKIE);
                context.Response.Redirect(context.Request.Path + context.Request.QueryString);
            }
        }

    }
}