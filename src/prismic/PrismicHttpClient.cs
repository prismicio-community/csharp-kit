using System;

using System.Net;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class PrismicHttpClient
	{
		private HttpClient client;

		public PrismicHttpClient()
		{
			this.client = new HttpClient ();
		}

		public PrismicHttpClient(HttpClient client)
		{
			if (client == null) {
				this.client = new HttpClient ();
			} else {
				this.client = client;
			}
		}

		public async Task<JToken> fetch(string url, ILogger logger, ICache cache)
		{
			JToken fromCache = cache.Get (url);
			if (fromCache != null) {
				return await Task.FromResult(fromCache);
			} else {
				return await _fetch (url, logger, cache);
			}
		}

		private static Regex maxAgeRe = new Regex(@"max-age=(\d+)");

		private async Task<JToken> _fetch(string url, ILogger logger, ICache cache) {
			var response = await this.client.GetAsync(url);
			var body = await response.Content.ReadAsStringAsync();
			switch (response.StatusCode) {
			case HttpStatusCode.OK:
				var json = JToken.Parse (body);
				var maxAgeValue = ""; // TODO response.Headers.GetValues ("max-age").FirstOrDefault ();
				var maxAge = maxAgeRe.Match (maxAgeValue);
				if (maxAge.Success) {
					long ttl = long.Parse (maxAge.Groups [1].Value);
					cache.Set (url, ttl, json);
				}
				return json;
			case HttpStatusCode.Unauthorized:
				var errorText = (string)JObject.Parse(body)["error"];
				if (errorText == "Invalid access token") {
					throw new Error(Error.ErrorCode.INVALID_TOKEN, errorText);
				} else {
					throw new Error(Error.ErrorCode.AUTHORIZATION_NEEDED, errorText);
				}
			default:
				throw new Error (Error.ErrorCode.UNEXPECTED, body);
			}

		}

	}

}

