using System;

using System.Net;
using System.Runtime.Serialization.Json;

namespace prismic
{
	public static class HttpClient
	{

		public static HttpWebResponse fetch(String url, ILogger logger, ICache cache)
		{
			// TODO: Cache

			HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
			using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
			{
				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(String.Format(
						"Server error (HTTP {0}: {1}).",
						response.StatusCode,
						response.StatusDescription));
				return response;
			}
		}

	}
}

