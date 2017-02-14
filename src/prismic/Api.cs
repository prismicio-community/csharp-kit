using System;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace prismic
{
	public class Api
	{
		public const string PREVIEW_COOKIE = "io.prismic.preview";
		public const string EXPERIMENT_COOKIE = "io.prismic.experiment";

		private PrismicHttpClient prismicHttpClient;
		public PrismicHttpClient PrismicHttpClient {
			get {
				return prismicHttpClient;
			}
		}
		private ApiData apiData;
		private String accessToken;
		public String AccessToken {
			get { return accessToken; }
		}
		private ICache cache;
		public ICache Cache {
			get {
				return cache;
			}
		}
		private ILogger logger;
		public ILogger Logger {
			get {
				return logger;
			}
		}
		public IList<Ref> Refs {
			get {
				return apiData.Refs;
			}
		}
		public IDictionary<String, Form> Forms {
			get {
				return apiData.Forms;
			}
		}
		public IDictionary<String, String> Bookmarks {
			get {
				return apiData.Bookmarks;
			}
		}
		public IDictionary<String, String> Types {
			get {
				return apiData.Types;
			}
		}
		public IList<String> Tags {
			get {
				return apiData.Tags;
			}
		}
		
		public Experiments Experiments {
			get {
				return apiData.Experiments;
			}
		}

		public Api(ApiData apiData, String accessToken, ICache cache, ILogger logger, PrismicHttpClient client) {
			this.apiData = apiData;
			this.accessToken = accessToken;
			this.cache = cache;
			this.logger = logger;
			this.prismicHttpClient = client;
		}

		public Ref Ref(String label) {
			foreach (Ref r in Refs) {
				if (r.Label == label)
					return r;
			}
			return null;
		}

		public Ref Master {
			get {
				foreach (Ref r in Refs) {
					if (r.IsMasterRef)
						return r;
				}
				return null;
			}
		}

		public Form.SearchForm Form(String form) {
			return new Form.SearchForm (this, Forms [form]);
		}

		/**
		* Entry point to get an {@link Api} object.
		* Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api", null, new Cache.BuiltInCache(999), new Logger.PrintlnLogger());</code>
		*
		* @param endpoint the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		* @param accessToken Your Oauth access token if you wish to use one (to access future content releases, for instance)
		* @param cache instance of a class that implements the {@link Cache} interface, and will handle the cache
		* @param logger instance of a class that implements the {@link Logger} interface, and will handle the logging
		* @return the usable API object
		*/
		public static async Task<Api> Get(String endpoint, String accessToken, ICache cache, ILogger logger, HttpClient client) {
			String url = (accessToken == null ? endpoint : (endpoint + "?access_token=" + WebUtility.UrlEncode(accessToken)));

			PrismicHttpClient prismicHttpClient = new PrismicHttpClient(client);
			JToken json = cache.Get(url);

            if (json == null)
			{
				json = await prismicHttpClient.fetch(url, logger, cache);
				cache.Set(url, 5000L, json);
			}

            ApiData apiData = ApiData.Parse(json);

            return new Api(apiData, accessToken, cache, logger, prismicHttpClient);
		}

		public static Task<Api> Get(String endpoint, String accessToken, ICache cache, ILogger logger) {
			return Get (endpoint, accessToken, cache, logger, null);
		}

		public static Task<Api> Get(String url, ICache cache) {
			return Get(url, null, cache, new NoLogger());
		}

		public static Task<Api> Get(String url, ICache cache, ILogger logger) {
			return Get(url, null, cache, logger);
		}

		/**
		* Entry point to get an {@link Api} object.
		* Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api", null);</code>
		*
		* @param url the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		* @param accessToken Your Oauth access token if you wish to use one (to access future content releases, for instance)
		* @return the usable API object
		*/
		public static Task<Api> Get(String url, String accessToken) {
			return Get(url, accessToken, new DefaultCache(), new NoLogger());
		}

		public static Task<Api> Get(String url, String accessToken, HttpClient client) {
			return Get(url, accessToken, new DefaultCache(), new NoLogger(), client);
		}

		/**
		* Entry point to get an {@link Api} object.
		* Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api");</code>
		*
		* @param url the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		* @return the usable API object
		*/
		public static Task<Api> Get(String url) {
			return Get(url, null, new DefaultCache(), new NoLogger());
		}

		public static Task<Api> Get(String url, HttpClient client) {
			return Get(url, null, new DefaultCache(), new NoLogger(), client);
		}

		/**
		* Return the URL to display a given preview
		* @param token as received from Prismic server to identify the content to preview
		* @param linkResolver the link resolver to build URL for your site
		* @param defaultUrl the URL to default to return if the preview doesn't correspond to a document
		*                (usually the home page of your site)
		* @return the URL you should redirect the user to preview the requested change
		*/
		public async Task<String> PreviewSession(String token, DocumentLinkResolver linkResolver, String defaultUrl) {
			var tokenJson = await this.prismicHttpClient.fetch(token, logger, cache);
			var mainDocumentId = tokenJson["mainDocument"];
			if (mainDocumentId == null) {
				return (defaultUrl);
			}
			var resp = await Form ("everything")
				.Query (Predicates.at ("document.id", mainDocumentId.ToString ()))
				.Ref (token)
				.Submit ();
			if (resp.Results.Count == 0) {
				return defaultUrl;
			}
			return linkResolver.Resolve (resp.Results[0]);
		}

	}

	public class ApiData
	{
		private IList<Ref> refs;
		public IList<Ref> Refs {
			get {
				return refs;
			}
		}

		private IDictionary<String,String> bookmarks;
		public IDictionary<String,String> Bookmarks {
			get {
				return bookmarks;
			}
		}

		private IDictionary<String,String> types;
		public IDictionary<String,String> Types {
			get {
				return types;
			}
		}

		private IList<String> tags;
		public IList<String> Tags {
			get {
				return tags;
			}
		}

		private IDictionary<String,Form> forms;
		public IDictionary<String,Form> Forms {
			get {
				return forms;
			}
		}

		private String oauthInitiateEndpoint;
		public String OAuthInitiateEndpoint {
			get {
				return oauthInitiateEndpoint;
			}
		}

		private String oauthTokenEndpoint;
		public String OAuthTokenEndpoint {
			get {
				return oauthTokenEndpoint;
			}
		}

		private Experiments experiments;
		public Experiments Experiments {
			get {
				return experiments;
			}
		}

		public ApiData(IList<Ref> refs,
			IDictionary<String,String> bookmarks,
			IDictionary<String,String> types,
			IList<String> tags,
			IDictionary<String,Form> forms,
			Experiments experiments,
			String oauthInitiateEndpoint,
			String oauthTokenEndpoint) {
			this.refs = refs;
			this.bookmarks = bookmarks;
			this.types = types;
			this.tags = tags;
			this.forms = forms;
			this.experiments = experiments;
			this.oauthInitiateEndpoint = oauthInitiateEndpoint;
			this.oauthTokenEndpoint = oauthTokenEndpoint;
		}

		// --

		public static ApiData Parse(JToken json) {
			IList<Ref> refs = json ["refs"].Select (r => Ref.Parse ((JObject)r)).ToList ();

			IDictionary<String, String> bookmarks = new Dictionary<String, String> ();
			foreach (KeyValuePair<String, JToken> bk in ((JObject)json ["bookmarks"])) {
				bookmarks [bk.Key] = (string)bk.Value;
			}

			var types = new Dictionary<String,String>();
			foreach (KeyValuePair<String, JToken> t in ((JObject)json ["types"])) {
				types [t.Key] = (string)t.Value;
			}

			IList<String> tags = json ["tags"].Select (r => (string)r).ToList ();

			var forms = new Dictionary<String,Form>();
			foreach (KeyValuePair<String, JToken> t in ((JObject)json ["forms"])) {
				forms [t.Key] = Form.Parse((JObject)t.Value);
			}

			var oauthInitiateEndpoint = (string)json["oauth_initiate"];
			var oauthTokenEndpoint = (string)json["oauth_token"];

			var experiments = Experiments.Parse(json["experiments"]);

			return new ApiData(refs, bookmarks, types, tags, forms, experiments, oauthInitiateEndpoint, oauthTokenEndpoint);
		}

	}

}

