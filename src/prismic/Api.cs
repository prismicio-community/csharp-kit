using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace prismic
{
    public class Api
    {
        public const string PREVIEW_COOKIE = "io.prismic.preview";
        public const string EXPERIMENT_COOKIE = "io.prismic.experiment";

        private readonly PrismicHttpClient _prismicHttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiData apiData;
        public IList<Ref> Refs => apiData.Refs;
        public IDictionary<string, Form> Forms => apiData.Forms;
        public IDictionary<string, string> Bookmarks => apiData.Bookmarks;
        public IDictionary<string, string> Types => apiData.Types;
        public IList<string> Tags => apiData.Tags;
        public Experiments Experiments => apiData.Experiments;

        private const string _documentId = "document.id";
        private const string _everything = "everything";

        public Api(ApiData apiData, PrismicHttpClient client)
        {
            this.apiData = apiData;
            _prismicHttpClient = client;
        }

        public Api(ApiData apiData, PrismicHttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            this.apiData = apiData;
            _prismicHttpClient = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public Ref Ref(string label) 
            => Refs.FirstOrDefault(r => r.Label == label);

        public Ref Master
            => Refs.FirstOrDefault(r => r.IsMasterRef);

        public Form.SearchForm Form(string form)
            => new Form.SearchForm(_prismicHttpClient, Forms[form])
                .Ref(GetCurrentReference());

        public Form.SearchForm Query(string q)
            => Form(_everything)
                .Ref(GetCurrentReference())
                .Query(q);

        public Form.SearchForm Query(params IPredicate[] predicates)
            => Form(_everything)
                .Ref(GetCurrentReference())
                .Query(predicates);

        /**
         * Retrieve multiple documents from their IDS
         */
        public Form.SearchForm GetByIDs(IEnumerable<string> ids, string reference = null, string lang = null)
            => Query(Predicates.In(_documentId, ids))
                .Ref(SetOrGetCurrentReference(reference))
                .Lang(lang);

        /**
         * Return the first document matching the predicate
         */
        public async Task<Document> QueryFirst(IPredicate p, string reference = null, string lang = null)
        {
            var response = await Query(p)
                .Ref(SetOrGetCurrentReference(reference))
                .Lang(lang)
                .Submit();

            var results = response.Results;

            if (results.Count() > 0)
            {
                return results[0];
            }
            else
            {
                return null;
            }
        }

        /**
         * Retrieve a document by its ID on the given reference
         *
         * @return the document, or null if it doesn't exist
         */
        public Task<Document> GetByID(string documentId, string reference = null, string lang = null)
            => QueryFirst(Predicates.At(_documentId, documentId), reference, lang);

        /**
         * Retrieve a document by its UID on the given reference
         *
         * @return the document, or null if it doesn't exist
         */
        public Task<Document> GetByUID(string documentType, string documentUid, string reference = null, string lang = null)
            => QueryFirst(Predicates.At("my." + documentType + ".uid", documentUid), reference, lang);


        public Task<Document> GetBookmark(string bookmark, string reference = null)
            => GetByID(apiData.Bookmarks[bookmark], reference);

        /**
        * Return the URL to display a given preview
        * @param token as received from Prismic server to identify the content to preview
        * @param linkResolver the link resolver to build URL for your site
        * @param defaultUrl the URL to default to return if the preview doesn't correspond to a document
        *                (usually the home page of your site)
        * @return the URL you should redirect the user to preview the requested change
        */
        public async Task<string> PreviewSession(string token, DocumentLinkResolver linkResolver, string defaultUrl)
        {
            var tokenJson = await _prismicHttpClient.Fetch(token);
            var mainDocumentId = tokenJson["mainDocument"];

            if (mainDocumentId == null)
                return defaultUrl;

            var resp = await GetByID(mainDocumentId.ToString(), token);

            if (resp == null)
                return defaultUrl;

            return linkResolver.Resolve(resp);
        }

        private string SetOrGetCurrentReference(string reference = null)
            => !string.IsNullOrWhiteSpace(reference)
                ? reference
                : GetCurrentReference();

        private string GetCurrentReference()
            => GetCookie(PREVIEW_COOKIE)
                ?? GetCookie(EXPERIMENT_COOKIE)
                ?? Master.Reference;

        private string GetCookie(string name)
        {
            if (_httpContextAccessor?.HttpContext == null)
                return null;

            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(name, out string cookieValue);

            if (!string.IsNullOrWhiteSpace(cookieValue))
                return cookieValue;

            return null;
        }
    }
}