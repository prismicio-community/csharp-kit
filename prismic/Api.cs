using System;

using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;

namespace prismic
{
	public class Api
	{

		private ApiData apiData;
		private String accessToken;
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

		public Api(ApiData apiData, String accessToken, ICache cache, ILogger logger) {
			this.apiData = apiData;
			this.accessToken = accessToken;
			this.cache = cache;
			this.logger = logger;
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
		public static Api Get(String endpoint, String accessToken, ICache cache, ILogger logger) {
			String url = (accessToken == null ? endpoint : (endpoint + "?access_token=" + HttpUtility.UrlEncode(accessToken)));
			HttpWebResponse httpResponse = HttpClient.fetch(url, logger, cache);

			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
			object response = jsonSerializer.ReadObject(httpResponse.GetResponseStream());
			return response as Api;

			/* TODO Reactive cache JsonNode json = cache.getOrSet(
				url,
				5000L,
				new Cache.Callback() {
					public JsonNode execute() {
						return HttpClient.fetch(url, logger, null);
					}
				}
			);

			ApiData apiData = ApiData.parse(json);
			return new Api(apiData, accessToken, cache, logger, fragmentParser);*/
		}

		public static Api Get(String url, ICache cache, ILogger logger) {
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
		public static Api Get(String url, String accessToken) {
			return Get(url, accessToken, new NoCache(), new NoLogger());
		}

		/**
		* Entry point to get an {@link Api} object.
		* Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api");</code>
		*
		* @param url the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		* @return the usable API object
		*/
		public static Api Get(String url) {
			return Get(url, null);
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
			List<String> tags,
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
		/*
		static ApiData parse(JsonValue json) {
			var refs = new List<Ref>();
			var refsJson = json.withArray("refs").elements();
			while(refsJson.hasNext()) {
				refs.add(Ref.parse(refsJson.next()));
			}

			var bookmarks = new Dictionary<String,String>();
			var bookmarksJson = json.with("bookmarks").fieldNames();
			while(bookmarksJson.hasNext()) {
				String bookmark = bookmarksJson.next();
				bookmarks.put(bookmark, json.with("bookmarks").path(bookmark).asText());
			}

			var types = new Dictionary<String,String>();
			var typesJson = json.with("types").fieldNames();
			while(typesJson.hasNext()) {
				var type = typesJson.next();
				types.put(type, json.with("types").path(type).asText());
			}

			var tags = new List<String>();
			var tagsJson = json.withArray("tags").elements();
			while(tagsJson.hasNext()) {
				tags.add(tagsJson.next().asText());
			}

			var forms = new Dictionary<String,Form>();
			var formsJson = json.with("forms").fieldNames();
			while(formsJson.hasNext()) {
				var form = formsJson.next();
				forms.put(form, Form.parse(json.with("forms").path(form)));
			}

			var oauthInitiateEndpoint = json.path("oauth_initiate").asText();
			var oauthTokenEndpoint = json.path("oauth_token").asText();

			var experiments = Experiments.parse(json.path("experiments"));

			return new ApiData(refs, bookmarks, types, tags, forms, experiments, oauthInitiateEndpoint, oauthTokenEndpoint);
		}
*/
	}

}

