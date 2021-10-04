using prismic.Middleware;

namespace Microsoft.AspNetCore.Builder
{
    public static class PrismicMiddlewareExtensions 
    {
        public static IApplicationBuilder UsePrismicPreviewExpiredMiddleware(this IApplicationBuilder app) 
            => app.UseMiddleware<PreviewExpiredExceptionHandlerMiddleware>();
    }
}