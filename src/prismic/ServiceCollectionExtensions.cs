using prismic;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PrismicServiceCollectionExtensions
    {
        public static IServiceCollection AddPrismic(this IServiceCollection services)
            => services.AddPrismic<DefaultPrismicApiAccessor>();

        public static IServiceCollection AddPrismic<TPrismicApiAccessor>(this IServiceCollection services)
           where TPrismicApiAccessor : class, IPrismicApiAccessor
        {
            HttpServiceCollectionExtensions.AddHttpContextAccessor(services);
            services.TryAddSingleton<ICache, InMemoryCache>();
            services.AddHttpClient<PrismicHttpClient>();
            services.TryAddSingleton<IPrismicApiAccessor, TPrismicApiAccessor>();

            return services;
        }
    }
}