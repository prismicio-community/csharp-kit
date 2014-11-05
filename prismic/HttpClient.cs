using System;

using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using prismic.extensions;

namespace prismic
{
	public static class HttpClient
	{

		public static Task<String> fetch(String url, ILogger logger, ICache cache)
		{
			// TODO: Cache
			HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
			Task<WebResponse> responseTask = request.GetResponseAsync ();
			var r = new TaskCompletionSource<String>();

			responseTask.ContinueWith(self => {
				if (self.IsFaulted) {
					if (self.Exception.InnerException is WebException) {
						var wex = (WebException)self.Exception.InnerException;
						switch (wex.Status) {
						case WebExceptionStatus.ProtocolError:
							HttpWebResponse response = (HttpWebResponse)wex.Response;
							StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
							var body = reader.ReadToEnd();
							switch (response.StatusCode) {
							case HttpStatusCode.Unauthorized:
								var errorText = (string)JObject.Parse(body)["error"];
								if (errorText == "Invalid access token") {
									r.SetException(new Error(Error.ErrorCode.INVALID_TOKEN, errorText));
								} else {
									r.SetException(new Error(Error.ErrorCode.AUTHORIZATION_NEEDED, errorText));
								}
								break;
							default:
								r.SetException(wex);
								break;
							}
							break;
						default:
							r.SetException(wex);
							break;
						}
					} else {
						r.SetException(self.Exception.InnerExceptions);
					}
				} else if (self.IsCanceled)
					r.SetCanceled();
				else try {
					var response = self.Result as HttpWebResponse;
					StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
					r.SetResult(reader.ReadToEnd());
				} catch (Exception e) {
					r.SetException(e);
				}
			});
			return r.Task;
		}

	}

}

