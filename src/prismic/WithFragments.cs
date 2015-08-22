using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using prismic.fragments;
using Group = prismic.fragments.Group;

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
			
            if (frag is Text) {
				return ((Text)frag).Value;
			}
			
            if (frag is Color) {
				return ((Color)frag).Hex;
			}
			
            if (frag is StructuredText) {
				var result = "";
				foreach (StructuredText.Block block in ((StructuredText)frag).Blocks) {
					if (block is StructuredText.TextBlock) {
						result += ((StructuredText.TextBlock)block).Text;
					}
				}
				return result;
			}
			
            return null;
		}

        public Number GetNumber(String field, IFormatProvider format = null)
        {
            if (format == null)
                format = Thread.CurrentThread.CurrentCulture;
            
            var frag = Get (field) as Text;

            if (frag == null)
                return null;

		    return Number.Parse(frag.Value, format);
		}

		public Image.View GetImageView(String field, String view) {
			var image = GetImage (field);
			if (image != null)
				return image.GetView (view);
			return null;
		}

		public Image GetImage(String field) {
			Fragment frag = Get (field);
			return frag is Image ? (Image)frag : null;
		}

		public Link GetLink(String field) {
			Console.WriteLine ("Get link for " + field);
			foreach (String key in Fragments.Keys) {
				Console.WriteLine ("Available = " + key);
			}
			Fragment frag = Get (field);
			return frag is Link ? (Link)frag : null;
		}

		public Date GetDate(String field) {
			Fragment frag = Get (field);
			return frag is Date ? (Date)frag : null;
		}

		public Timestamp GetTimestamp(String field) {
			Fragment frag = Get (field);
			return frag is Timestamp ? (Timestamp)frag : null;
		}

		public Embed GetEmbed(String field) {
			Fragment frag = Get (field);
			return frag is Embed ? (Embed)frag : null;
		}

		public Group GetGroup(String field) {
			Fragment frag = Get (field);
			return frag is Group ? (Group)frag : null;
		}

		public Color GetColor(String field) {
			Fragment frag = Get (field);
			return frag is Color ? (Color)frag : null;
		}

		public GeoPoint GetGeoPoint(String field) {
			Fragment frag = Get (field);
			return frag is GeoPoint ? (GeoPoint)frag : null;
		}

		public StructuredText GetStructuredText(String field) {
			Fragment frag = Get (field);
			return frag is StructuredText ? (StructuredText)frag : null;
		}

		public String GetHtml(String field, DocumentLinkResolver resolver) {
			return GetHtml (field, resolver);
		}

		public String GetHtml(String field, DocumentLinkResolver resolver, HtmlSerializer serializer) {
			Fragment fragment = Get(field);
			if (fragment == null)
				return "";
			if (fragment is StructuredText) {
				return ((StructuredText)fragment).AsHtml(resolver, serializer);
			}
			if(fragment is Number) {
				return ((Number)fragment).AsHtml();
			}
			if(fragment is Color) {
				return ((Color)fragment).AsHtml();
			}
			if(fragment is Text) {
				return ((Text)fragment).AsHtml();
			}
			if(fragment is Date) {
				return ((Date)fragment).AsHtml();
			}
			if(fragment is Embed) {
				return ((Embed)fragment).AsHtml();
			}
			else if(fragment is Image) {
				return ((Image)fragment).AsHtml(resolver);
			}
			else if(fragment is WebLink) {
				return ((WebLink)fragment).AsHtml();
			}
			else if(fragment is DocumentLink) {
				return ((DocumentLink)fragment).AsHtml(resolver);
			}
			else if(fragment is Group) {
				return ((Group)fragment).AsHtml(resolver);
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
				if (fragment is DocumentLink) {
					result.Add ((DocumentLink)fragment);
				} else if (fragment is StructuredText) {
					var text = (StructuredText)fragment;
					foreach (StructuredText.Block block in text.Blocks) {
						if (block is StructuredText.TextBlock) {
							var spans = ((StructuredText.TextBlock)block).Spans;
							foreach (StructuredText.Span span in spans) {
								if (span is StructuredText.Hyperlink) {
									var link = ((StructuredText.Hyperlink)span).Link;
									if (link is DocumentLink) {
										result.Add ((DocumentLink)link);
									}
								}
							}
						}
					}
				} else if (fragment is Group) {
					var group = (Group)fragment;
					foreach (GroupDoc doc in group.GroupDocs) {
						result.AddRange (doc.LinkedDocuments());
					}
				}
			}
			return result;
		}

	}
}

