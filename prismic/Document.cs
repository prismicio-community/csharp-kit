using System;

using System.Collections.Generic;

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
		private String type;
		public String Type {
			get {
				return type;
			}
		}
		private IList<LinkedDocument> linkedDocuments;
		public IList<LinkedDocument> LinkedDocuments {
			get {
				return linkedDocuments;
			}
		}

		public Document(String id, String type, String href, ISet<String> tags, IList<String> slugs, IList<LinkedDocument> linkedDocuments, IDictionary<String,Fragment> fragments): base(fragments) {
			this.id = id;
			this.type = type;
			this.href = href;
			this.tags = tags;
			this.slugs = slugs;
			this.linkedDocuments = linkedDocuments;
		}

		public fragments.DocumentLink AsDocumentLink() {
			return new fragments.DocumentLink(id, type, tags, slugs[0], false);
		}

	}
}

