using System;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class LinkedDocument
	{
		private String id;
		public String Id {
			get { return id; }
		}
		private String slug;
		public String Slug {
			get { return slug; }
		}
		private String type;
		public String Type {
			get { return type; }
		}
		private ISet<String> tags;
		public ISet<String> Tags {
			get { return tags; }
		}

		public LinkedDocument(String id, String slug, String type, ISet<String> tags) {
			this.id = id;
			this.slug = slug;
			this.type = type;
			this.tags = tags;
		}

		public static LinkedDocument Parse(JObject json) {
			String id = (string)json["id"];
			String slug = json["slug"] != null ? (string)json["slug"] : null;
			String type = (string)json["type"];
			ISet<String> tags = new HashSet<String>(json ["tags"].Select (r => (string)r));
			return new LinkedDocument(id, slug, type, tags);
		}
	}
}

