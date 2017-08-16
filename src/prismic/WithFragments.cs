using System;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using prismic.fragments;

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
			Regex r = new Regex (Regex.Escape(field) + @"\[\d+\]");
			IList<Fragment> result = new List<Fragment>();
			foreach(KeyValuePair<String,Fragment> entry in Fragments) {
				if(r.Match(entry.Key).Success) {
					result.Add(entry.Value);
				}
			}
			return result;
		}

		public Fragment Get(String field) {
			Fragment single = null;
			Fragments.TryGetValue(field, out single);
			if (single == null) {
				IList<Fragment> multi = GetAll(field);
				if (multi.Count > 0) {
					single = multi [0];
				}
			}
			return single;
		}

		public String GetText(String field) {
			Fragment frag = Get (field);
			if (frag is fragments.Text) {
				return ((fragments.Text)frag).Value;
			}
			if (frag is fragments.Number) {
				return ((fragments.Number)frag).Value.ToString ();
			}
			if (frag is fragments.Color) {
				return ((fragments.Color)frag).Hex;
			}
			if (frag is fragments.StructuredText) {
				var result = "";
				foreach (fragments.StructuredText.Block block in ((fragments.StructuredText)frag).Blocks) {
					if (block is fragments.StructuredText.TextBlock) {
						result += ((fragments.StructuredText.TextBlock)block).Text;
					}
				}
				return result;
			}
			if (frag is fragments.Number) {
				return ((fragments.Number)frag).Value.ToString ();
			}
			return null;
		}

		public fragments.Number GetNumber(String field)
		{
			Fragment frag = Get(field);
			return frag is fragments.Number ? (fragments.Number)frag : null;
		}

		public fragments.SliceZone GetSliceZone(String field)
		{
			Fragment frag = Get(field);
			return frag is fragments.SliceZone ? (fragments.SliceZone)frag : null;
		}

		public fragments.Image.View GetImageView(String field, String view)
		{
			var image = GetImage (field);
			if (image != null)
				return image.GetView (view);
			return null;
		}

		public fragments.Image GetImage(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Image ? (fragments.Image)frag : null;
		}

		public fragments.Link GetLink(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Link ? (fragments.Link)frag : null;
		}

		public fragments.Date GetDate(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Date ? (fragments.Date)frag : null;
		}

		public fragments.Timestamp GetTimestamp(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Timestamp ? (fragments.Timestamp)frag : null;
		}

		public fragments.Embed GetEmbed(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Embed ? (fragments.Embed)frag : null;
		}

		public fragments.Group GetGroup(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Group ? (fragments.Group)frag : null;
		}

		public fragments.Color GetColor(String field) {
			Fragment frag = Get (field);
			return frag is fragments.Color ? (fragments.Color)frag : null;
		}

		public fragments.GeoPoint GetGeoPoint(String field) {
			Fragment frag = Get (field);
			return frag is fragments.GeoPoint ? (fragments.GeoPoint)frag : null;
		}

		public fragments.StructuredText GetStructuredText(String field) {
			Fragment frag = Get (field);
			return frag is fragments.StructuredText ? (fragments.StructuredText)frag : null;
		}

		public String GetHtml(String field, DocumentLinkResolver resolver) {
			return GetHtml (field, resolver, null);
		}

		public String GetHtml(String field, DocumentLinkResolver resolver, HtmlSerializer serializer) {
			Fragment fragment = Get(field);
			return WithFragments.GetHtml(fragment, resolver, serializer);
		}

		public static String GetHtml(Fragment fragment, DocumentLinkResolver resolver, HtmlSerializer serializer) {
			if (fragment == null)
				return "";
			if (fragment is fragments.StructuredText) {
				return ((fragments.StructuredText)fragment).AsHtml(resolver, serializer);
			}
			if(fragment is fragments.Number) {
				return ((fragments.Number)fragment).AsHtml();
			}
			if(fragment is fragments.Color) {
				return ((fragments.Color)fragment).AsHtml();
			}
			if(fragment is fragments.Text) {
				return ((fragments.Text)fragment).AsHtml();
			}
			if(fragment is fragments.Date) {
				return ((fragments.Date)fragment).AsHtml();
			}
			if(fragment is fragments.Embed) {
				return ((fragments.Embed)fragment).AsHtml();
			}
			else if(fragment is fragments.Image) {
				return ((fragments.Image)fragment).AsHtml(resolver);
			}
			else if(fragment is fragments.WebLink) {
				return ((fragments.WebLink)fragment).AsHtml();
			}
			else if(fragment is fragments.DocumentLink) {
				return ((fragments.DocumentLink)fragment).AsHtml(resolver);
			}
			else if(fragment is fragments.Group) {
				return ((fragments.Group)fragment).AsHtml(resolver);
			}
			else if (fragment is fragments.SliceZone) {
				return ((fragments.SliceZone)fragment).AsHtml(resolver);
			}

			return "";

		}

		public String AsHtml(DocumentLinkResolver linkResolver) {
			return AsHtml(linkResolver, null);
		}

		public String AsHtml(DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
			String html = "";
			foreach(KeyValuePair<String,Fragment> fragment in Fragments) {
				html += ("<section data-field=\"" + fragment.Key + "\">");
				html += GetHtml(fragment.Key, linkResolver, htmlSerializer);
				html += ("</section>");
			}
			return html.Trim();
		}

		public IList<DocumentLink> LinkedDocuments() {
			var result = new List<DocumentLink>();
			foreach(Fragment fragment in Fragments.Values) {
				if (fragment is fragments.DocumentLink) {
					result.Add ((fragments.DocumentLink)fragment);
				} else if (fragment is fragments.StructuredText) {
					var text = (fragments.StructuredText)fragment;
					foreach (fragments.StructuredText.Block block in text.Blocks) {
						if (block is fragments.StructuredText.TextBlock) {
							var spans = ((fragments.StructuredText.TextBlock)block).Spans;
							foreach (fragments.StructuredText.Span span in spans) {
								if (span is fragments.StructuredText.Hyperlink) {
									var link = ((fragments.StructuredText.Hyperlink)span).Link;
									if (link is fragments.DocumentLink) {
										result.Add ((fragments.DocumentLink)link);
									}
								}
							}
						}
					}
				} else if (fragment is fragments.Group) {
					var group = (fragments.Group)fragment;
					foreach (GroupDoc doc in group.GroupDocs) {
						result.AddRange (doc.LinkedDocuments());
					}
				}
			}
			return result;
		}

	}
}

