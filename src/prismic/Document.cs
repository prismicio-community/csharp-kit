using System;

using System.Web;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class Document: WithFragments
	{
		private String id;
		public String Id {
			get {
				return id;
			}
		}
		private String uid;
		public String Uid {
			get {
				return uid;
			}
		}
		private String href;
		public String Href {
			get {
				return href;
			}
		}
		private ISet<String> tags;
		public ISet<String> Tags {
			get {
				return tags;
			}
		}
		private IList<String> slugs;
		public IList<String> Slugs {
			get {
				return slugs;
			}
		}
		public String Slug {
			get {
				if (slugs.Count > 0) {
					return slugs [0];
				} else {
					return "-";
				}
			}
		}
		private String type;
		public String Type {
			get {
				return type;
			}
		}

		public Document(String id, String uid, String type, String href, ISet<String> tags, IList<String> slugs, IDictionary<String,Fragment> fragments): base(fragments) {
			this.id = id;
			this.uid = uid;
			this.type = type;
			this.href = href;
			this.tags = tags;
			this.slugs = slugs;
		}

		public fragments.DocumentLink AsDocumentLink() {
			return new fragments.DocumentLink(id, uid, type, tags, slugs[0], this.Fragments, false);
		}

		public static IDictionary<String, Fragment> parseFragments(JToken json) {
			IDictionary<String, Fragment> fragments = new Dictionary<String, Fragment>();

			if (json == null) {
				return fragments;
			}
			var type = (string)json["type"];

			if (json ["data"] == null) {
				return fragments;
			}
			foreach (KeyValuePair<String, JToken> field in ((JObject)json ["data"][type])) {
				if (field.Value is JArray) {
					var i = 0;
					foreach (JToken elt in ((JArray)field.Value)) {
						String fragmentName = type + "." + field.Key + "[" + i++ + "]";
						String fragmentType = (string)elt["type"];
						JToken fragmentValue = elt["value"];
						Fragment fragment = prismic.fragments.FragmentParser.Parse(fragmentType, fragmentValue);
						if (fragment != null) {
							fragments[fragmentName] = fragment;
						}
					}
				} else {
					String fragmentName = type + "." + field.Key;
					String fragmentType = (string)field.Value["type"];
					JToken fragmentValue = field.Value["value"];
					Fragment fragment = prismic.fragments.FragmentParser.Parse(fragmentType, fragmentValue);
					if (fragment != null) {
						fragments [fragmentName] = fragment;
					}
				}
			}
			return fragments;
		}

		public static Document Parse(JToken json) {
			var id = (string)json["id"];
			var uid = (string)json["uid"];
			var href = (string)json["href"];
			var type = (string)json["type"];

			ISet<String> tags = new HashSet<String>(json ["tags"].Select (r => (string)r));
			IList<String> slugs = json ["slugs"].Select (r => HttpUtility.UrlDecode((string)r)).ToList ();

			IDictionary<String, Fragment> fragments = parseFragments (json);

			return new Document(id, uid, type, href, tags, slugs, fragments);
		}


	}
}

