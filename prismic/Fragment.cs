using System;

using System.Collections.Generic;

namespace prismic
{
	public interface Fragment
	{
	}

	namespace fragments {

		public class Text: Fragment {
			private String value;
			public String Value {
				get {
					return value;
				}
			}
			public Text(String value) {
				this.value = value;
			}
			public String AsHtml() {
				return ("<span class=\"text\">" + value + "</span>");
			}
		}

		public class Number: Fragment {
			private String value;
			public String Value {
				get {
					return value;
				}
			}
			public Number(String value) {
				this.value = value;
			}
			public String AsHtml() {
				return ("<span class=\"number\">" + value + "</span>");
			}
		}

		public class Image: Fragment {

			public class View {
				private String url;
				public String Url {
					get {
						return url;
					}
				}
				private int width;
				public int Width {
					get {
						return width;
					}
				}
				private int height;
				public int Height {
					get {
						return height;
					}
				}
				private String alt;
				public String Alt {
					get {
						return alt;
					}
				}
				private String copyright;
				public String Copyright {
					get {
						return copyright;
					}
				}
				private Link linkTo;
				public Link LinkTo {
					get {
						return linkTo;
					}
				}

				public Double Ratio {
					get {
						return width / height;
					}
				}

				public View(String url, int width, int height, String alt, String copyright, Link linkTo) {
					this.url = url;
					this.width = width;
					this.height = height;
					this.alt = alt;
					this.copyright = copyright;
					this.linkTo = linkTo;
				}

				public String AsHtml(DocumentLinkResolver linkResolver) {
					String imgTag = "<img alt=\"" + alt + "\" src=\"" + url + "\" width=\"" + width + "\" height=\"" + height + "\" />";
					if (this.linkTo != null) {
						String u = "about:blank";
						if (this.linkTo is WebLink) {
							u = ((WebLink) this.linkTo).Url;
						} else if (this.linkTo is ImageLink) {
							u = ((ImageLink) this.linkTo).Url;
						} else if (this.linkTo is DocumentLink) {
							u = ((DocumentLink)this.linkTo).IsBroken
								? "#broken"
								: linkResolver.Resolve((DocumentLink) this.LinkTo);
						}
						return "<a href=\"" + u + "\">" + imgTag + "</a>";
					} else {
						return imgTag;
					}
				}

				//

				/* static View parse(JsonNode json) {
					String url = json.path("url").asText();
					int width = json.with("dimensions").path("width").intValue();
					int height = json.with("dimensions").path("height").intValue();
					String alt = json.path("alt").asText();
					String copyright = json.path("copyright").asText();
					Link linkTo = StructuredText.parseLink(json.path("linkTo"));
					return new View(url, width, height, alt, copyright, linkTo);
				}*/

			}

			private View main;
			private IDictionary<String, View> views;

			public Image(View main, IDictionary<String, View> views) {
				this.main = main;
				this.views = views;
			}

			public Image(View main): this(main, new Dictionary<String,View>()) {}

			public View GetView(String view) {
				if("main" == view) {
					return main;
				}
				return views[view];
			}

			public String asHtml(DocumentLinkResolver linkResolver) {
				return GetView("main").AsHtml(linkResolver);
			}

			// --
			/*
			static Image parse(JsonNode json) {
				View main = View.parse(json.with("main"));
				Map<String,View> views = new HashMap<String,View>();
				Iterator<String> viewsJson = json.with("views").fieldNames();
				while(viewsJson.hasNext()) {
					String view = viewsJson.next();
					views.put(view, View.parse(json.with("views").with(view)));
				}
				return new Image(main, views);
			}*/

		}

		public interface Link: Fragment {
			String GetUrl(DocumentLinkResolver resolver);
		}

		public class WebLink: Link {
			private String url;
			public String Url {
				get {
					return url;
				}
			}
			private String contentType;
			public String ContentType {
				get {
					return contentType;
				}
			}

			public WebLink(String url, String contentType) {
				this.url = url;
				this.contentType = contentType;
			}

			public String GetUrl(DocumentLinkResolver resolver) {
				return url;
			}

			public String AsHtml() {
				return ("<a href=\"" + url + "\">" + url + "</a>");
			}
		}

		public class FileLink: Link {
			private String url;
			public String Url {
				get {
					return Url;
				}
			}
			private String kind;
			public String Kind {
				get {
					return kind;
				}
			}

			private long size;
			public long Size {
				get {
					return size;
				}
			}

			private String filename;
			public String Filename {
				get {
					return filename;
				}
			}

			public FileLink(String url, String kind, long size, String filename) {
				this.url = url;
				this.kind = kind;
				this.size = size;
				this.filename = filename;
			}

			public String GetUrl(DocumentLinkResolver resolver) {
				return url;
			}
		}

		public class ImageLink: Link {
			private String url;
			public String Url {
				get {
					return url;
				}
			}

			public ImageLink(String url) {
				this.url = url;
			}

			public String GetUrl(DocumentLinkResolver resolver) {
				return url;
			}

			public String AsHtml() {
				return ("<a href=\"" + url + "\">" + url + "</a>");
			}
		}

		public class DocumentLink: Link {
			private String id;
			public String Id {
				get {
					return id;
				}
			}
			private String type;
			public String Type {
				get {
					return type;
				}
			}
			private ISet<String> tags;
			public ISet<String> Tags {
				get {
					return tags;
				}
			}
			private String slug;
			public String Slug {
				get {
					return slug;
				}
			}
			private Boolean broken;
			public Boolean IsBroken {
				get {
					return broken;
				}
			}

			public DocumentLink(String id, String type, ISet<String> tags, String slug, Boolean broken) {
				this.id = id;
				this.type = type;
				this.tags = tags;
				this.slug = slug;
				this.broken = broken;
			}

			public String GetUrl(DocumentLinkResolver resolver) {
				return resolver.Resolve (this);
			}

			public String asHtml(DocumentLinkResolver linkResolver) {
				return ("<a " + (linkResolver.GetTitle(this) == null ? "" : "title=\"" + linkResolver.GetTitle(this) + "\" ") + "href=\"" + linkResolver.Resolve(this) + "\">" + slug + "</a>");
			}
		}

		public class Date: Fragment {
			private DateTime value;
			public DateTime Value {
				get {
					return value;
				}
			}
			public Date(DateTime value) {
				this.value = value;
			}
		}

		public class Timestamp: Fragment {
			private DateTime value;
			public DateTime Value {
				get {
					return value;
				}
			}
			public Timestamp(DateTime value) {
				this.value = value;
			}
		}

		public class Embed: Fragment {
			private String type;
			public String Type {
				get {
					return type;
				}
			}
			private String provider;
			public String Provider {
				get {
					return provider;
				}
			}
			private String url;
			public String Url {
				get {
					return url;
				}
			}
			private int width;
			public int Width {
				get {
					return width;
				}
			}
			private int height;
			public int Height {
				get {
					return height;
				}
			}
			private String html;
			public String Html {
				get {
					return html;
				}
			}

			public Embed(String type, String provider, String url, int width, int height, String html) {
				this.type = type;
				this.provider = provider;
				this.url = url;
				this.width = width;
				this.height = height;
				this.html = html;
			}
		}

		public class Group: Fragment {
			private IList<GroupDoc> groupDocs;
			public IList<GroupDoc> GroupDocs {
				get {
					return groupDocs;
				}
			}

			public Group(IList<GroupDoc> groupDocs) {
				this.groupDocs = groupDocs;
			}

			public String AsHtml(DocumentLinkResolver linkResolver) {
				var html = "";
				foreach (GroupDoc groupDoc in this.groupDocs) {
					html += groupDoc.AsHtml(linkResolver);
				}
				return html;
			}
		}



	}


}

