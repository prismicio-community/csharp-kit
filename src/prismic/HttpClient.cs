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
	public static class HttpClient
	{

		public static async Task<JToken> fetch(string url, ILogger logger, ICache cache)
		{
			JToken fromCache = cache.Get (url);
			if (fromCache != null) {
				return await Task.FromResult(fromCache);
			} else {
				return await _fetch (url, logger, cache);
			}
		}

		private static Regex maxAgeRe = new Regex(@"max-age=(\d+)");

		private static async Task<JToken> _fetch(string url, ILogger logger, ICache cache) {
			var client = new System.Net.Http.HttpClient ();
			var response = await client.GetAsync(url);
			var body = await response.Content.ReadAsStringAsync();
			switch (response.StatusCode) {
			case HttpStatusCode.OK:
				var json = JToken.Parse (body);
				var maxAgeValue = ""; // TODO response.Headers.GetValues ("max-age").FirstOrDefault ();
				var maxAge = maxAgeRe.Match (maxAgeValue);
				if (maxAge.Success) {
					long ttl = long.Parse (maxAge.Groups [1].Value);
					Console.WriteLine ("Got a ttl of: " + ttl);
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

