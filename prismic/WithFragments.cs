using System;

using System.Collections.Generic;

namespace prismic
{
	public abstract class WithFragments
	{
		private IDictionary<String, Fragment> fragments;
		public IDictionary<String, Fragment> Fragments {
			get {
				return fragments;
			}
		}

		public WithFragments (IDictionary<String, Fragment> fragments)
		{
			this.fragments = fragments;
		}

		public fragments.Text GetText(String field) {
			return null; // TODO
		}

		public fragments.Number GetNumber(String field) {
			return null; // TODO
		}

		public fragments.Image.View GetImageView(String field, String view) {
			return null; // TODO
		}

		public fragments.Date GetDate(String field) {
			return null; // TODO
		}

		public fragments.Timestamp GetTimestamp(String field) {
			return null; // TODO
		}

		public fragments.Embed GetEmbed(String field) {
			return null; // TODO
		}

		public fragments.Group GetGroup(String field) {
			return null; // TODO
		}

		public fragments.StructuredText GetStructuredText(String field) {
			return null; // TODO
		}

		public String GetHtml(String field, DocumentLinkResolver resolver) {
			return GetHtml (field, resolver);
		}

		public String GetHtml(String field, DocumentLinkResolver resolver, HtmlSerializer serializer) {
			return ""; // TODO
		}

		public String AsHtml(DocumentLinkResolver linkResolver) {
			return AsHtml(linkResolver, null);
		}

		public String AsHtml(DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
			String html = "";
			foreach(KeyValuePair<String,Fragment> fragment in Fragments) {
				html += ("<section data-field=\"" + fragment.Key + "\">");
				html += GetHtml(fragment.Key, linkResolver, htmlSerializer);
				html += ("</section>\n");
			}
			return html.Trim();
		}

	}
}

