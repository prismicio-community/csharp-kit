using System;

using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using prismic.extensions;

namespace prismic
{
	public static class HttpClient
	{

		public static Task<String> fetch(String url, ILogger logger, ICache cache)
		{
			var result = new TaskCompletionSource<String>();
			// TODO: Cache
			HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
			Task<WebResponse> responseTask = request.GetResponseAsync ();
			return responseTask.Select(r => {
				var response = r as HttpWebResponse; 
				// TODO: Correct error management
				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(String.Format("Server error (HTTP {0}: {1}).",
					response.StatusCode,
					response.StatusDescription));
				StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				return reader.ReadToEnd();
			});
		}

	}

}

