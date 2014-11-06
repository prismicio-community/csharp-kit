using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace prismic
{

	public interface ICache {

		void Set(string key, long ttl, JToken item);

		JToken Get (string key);

	}

	public class NoCache: ICache
	{
		public void Set (string key, long ttl, JToken item) {}

		public JToken Get(string key) {
			return null;
		}
	}

	// An easy way to create ICache implementation with lambdas
	public class LambdaCache: ICache
	{
		private readonly System.Func<string, long, JToken, object> set;
		private readonly System.Func<string, JToken> get;

		public LambdaCache(System.Func<string, long, JToken, object> set, System.Func<string, JToken> get) {
			this.set = set;
			this.get = get;
		}

		public void Set (string key, long ttl, JToken item) {
			this.set (key, ttl, item);
		}

		public JToken Get(string key) {
			return this.get(key);
		}

		public static LambdaCache For(System.Func<string, long, JToken, object> set, System.Func<string, JToken> get) {
			return new LambdaCache (set, get);
		}
	}

	// An LRU implementation for the Cache - recommended
	public class DefaultCache: ICache
	{
		private IDictionary<String, CacheEntry> data = new Dictionary<string, CacheEntry> ();
		private LinkedList<String> lruPriority = new LinkedList<string> ();
		private int maxSize;

		public DefaultCache(int maxSize) {
			this.maxSize = maxSize;
		}

		public DefaultCache(): this(100) {}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Set (string key, long ttl, JToken item) {
			this.data [key] = new CacheEntry (item, ttl);
			this.lruPriority.AddLast (key);
			this.cleanup ();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public JToken Get(string key) {
			CacheEntry result = null;
			data.TryGetValue (key, out result);
			if (result == null) {
				return null;
			}
			if (DateTime.Compare (DateTime.Now, result.expiration) > 0) {
				// Expired: remove it completely
				this.removeFromLRU (key);
				this.data.Remove (key);
				return null;
			} else {
				// Found a result: put it at the end of the LRU before returning
				this.removeFromLRU (key);
				lruPriority.AddLast (key);
				return result.item;
			}
		}

		private void removeFromLRU(string key) {
			LinkedListNode<string> node = lruPriority.First;
			var found = false;
			while (node != null && !found) {
				var next = node.Next;
				if (node.Value == key) {
					found = true;
					lruPriority.Remove (key);
				}
				node = next;
			}
		}

		private void cleanup() {
			while (data.Count > maxSize) {
				this.data.Remove (lruPriority.First ());
				lruPriority.RemoveFirst ();
			}
		}

	}

	class CacheEntry {
		public readonly DateTime expiration;
		public readonly JToken item;
		public CacheEntry(JToken item, long ttl) {
			this.expiration = DateTime.Now.AddSeconds(ttl);
			this.item = item;
		}
	}

}

