using System.Threading.Tasks;

namespace prismic
{
    public interface IPrismicApiAccessor
    {
        Task<Api> GetApi();
        Task<Api> GetApi(string endpoint, string accessToken);
        Task<Api> GetApi(string endpoint);
    }
}
