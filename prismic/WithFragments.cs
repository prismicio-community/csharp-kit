using System;

using System.Collections.Generic;
using System.Text.RegularExpressions;

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

		public IList<Fragment> GetAll(String field) {
			Regex r = new Regex (@"\\Q" + field + "\\E\\[\\d+\\]");
			IList<Fragment> result = new List<Fragment>();
			foreach(KeyValuePair<String,Fragment> entry in Fragments) {
				if(r.Match(entry.Key).Success) {
					result.Add(entry.Value);
				}
			}
			return result;
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

		public fragments.Link GetLink(String field) {
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

		public fragments.Color GetColor(String field) {
			return null; // TODO
		}

		public fragments.GeoPoint GetGeoPoint(String field) {
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

