using System;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace prismic
{

	public interface ICache {

		void Set(string key, long ttl, JToken response);

		JToken Get (string key);

	}

	public class NoCache: ICache
	{
		public void Set (string key, long ttl, JToken response) {}

		public JToken Get(string key) {
			return null;
		}

	}


}

