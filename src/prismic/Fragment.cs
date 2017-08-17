using System;

using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web;

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
				return ("<span class=\"text\">" + HttpUtility.HtmlEncode(value) + "</span>");
			}

			public static Text Parse(JToken json) {
				return new Text((string)json);
			}
		}

		public class Number: Fragment {

			private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");
			private Decimal value;
			public Decimal Value {
				get {
					return value;
				}
			}
			public Number(Decimal value) {
				this.value = value;
			}
			public String AsHtml() {
				return ("<span class=\"number\">" + value + "</span>");
			}
			public static Number Parse(JToken json, CultureInfo ci = null)
			{
				if(ci == null)
					ci = _defaultCultureInfo;

				var v = Convert.ToDecimal((string) json, ci);
				return new Number (v);
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

				public static View Parse(JToken json) {
					String url = (string)json["url"];
					int width = (int)json["dimensions"]["width"];
					int height = (int)json["dimensions"]["height"];
					String alt = (string)json["alt"];
					String copyright = (string)json["copyright"];
					Link linkTo = (Link)StructuredText.ParseLink((JObject)json["linkTo"]);
					return new View(url, width, height, alt, copyright, linkTo);
				}

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

			public String AsHtml(DocumentLinkResolver linkResolver) {
				return GetView("main").AsHtml(linkResolver);
			}

			// --

			public static Image Parse(JToken json) {
				View main = View.Parse((JObject)json["main"]);
				var views = new Dictionary<String,View>();
				foreach (KeyValuePair<String, JToken> v in ((JObject)json ["views"])) {
					views [v.Key] = View.Parse((JObject)v.Value);
				}
				return new Image(main, views);
			}

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

			public static WebLink Parse(JToken json) {
				return new WebLink((string)json["url"], null);
			}

		}

		public class FileLink: Link {
			private String url;
			public String Url {
				get {
					return url;
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

			public static FileLink Parse(JToken json) {
				String url = (string)json["file"]["url"];
				String kind = (string)json["file"]["kind"];
				String size = (string)json["file"]["size"];
				String name = (string)json["file"]["name"];
				return new FileLink(url, kind, long.Parse(size), name);
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

			public static ImageLink Parse(JToken json) {
				return new ImageLink((string)json["image"]["url"]);
			}

		}

		public class DocumentLink: WithFragments, Link {
			private String id;
			public String Id {
				get {
					return id;
				}
			}
			private String uid;
			public String Uid
			{
				get
				{
					return uid;
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

			public DocumentLink(String id, String uid, String type, ISet<String> tags, String slug, IDictionary<String,Fragment> fragments, Boolean broken): base(fragments) {
				this.id = id;
				this.uid = uid;
				this.type = type;
				this.tags = tags;
				this.slug = slug;
				this.broken = broken;
			}

			public String GetUrl(DocumentLinkResolver resolver) {
				return resolver.Resolve (this);
			}

			public new String AsHtml(DocumentLinkResolver linkResolver) {
				return ("<a " + (linkResolver.GetTitle(this) == null ? "" : "title=\"" + linkResolver.GetTitle(this) + "\" ") + "href=\"" + linkResolver.Resolve(this) + "\">" + slug + "</a>");
			}

			public static DocumentLink Parse(JToken json) {
				JObject document = (JObject)json["document"];
				Boolean broken = (Boolean)json["isBroken"];
				string id = (string)document["id"];
				string type = (string)document["type"];
				string slug = (string)document["slug"];
				string uid = null;
				if (document["uid"] != null)
					uid = (string)document["uid"];
				ISet<String> tags;
				if (json["tags"] != null)
					tags = new HashSet<String>(json ["tags"].Select (r => (string)r));
				else
					tags = new HashSet<String> ();
				IDictionary<String, Fragment> fragments = Document.parseFragments (json["document"]);
				return new DocumentLink(id, uid, type, tags, slug, fragments, broken);
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
			public String AsHtml() {
				return ("<time>" + value + "</time>");
			}
			public static Date Parse(JToken json) {
				try {
					return new Date(DateTime.ParseExact((string)json, "yyyy-MM-dd", CultureInfo.InvariantCulture));
				} catch(FormatException) {
					return null;
				}
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
			public String AsHtml() {
				return ("<time>" + value + "</time>");
			}
			public static Timestamp Parse(JToken json) {
				return new Timestamp(json.ToObject<DateTime>());
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
			private int? width;
			public int? Width {
				get {
					return width;
				}
			}
			private int? height;
			public int? Height {
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
			private JObject oembedJson;
			public JObject OEmbedJson {
				get { return oembedJson; }
			}

			public Embed(String type, String provider, String url, int? width, int? height, String html, JObject oembedJson) {
				this.type = type;
				this.provider = provider;
				this.url = url;
				this.width = width;
				this.height = height;
				this.html = html;
				this.oembedJson = oembedJson;
			}

			public String AsHtml() {
				var providerAttr = provider != null ? ("\" data-oembed-provider=\"" + provider.ToLower () + "\"") : "";
				return ("<div data-oembed=\"" + url + "\" data-oembed-type=\"" + type.ToLower() + "\"" + providerAttr + ">" + html + "</div>");
			}

			public static Embed Parse(JToken json) {
				JObject oembedJson = (JObject)json["oembed"];
				String type = (string)oembedJson["type"];
				String provider = oembedJson["provider_name"] != null ? (string)oembedJson["provider_name"] : null;
				String url = (string)oembedJson["embed_url"];
				int? width = (oembedJson["width"] != null && oembedJson["width"].Type == JTokenType.Integer) ? (int?)oembedJson["width"] : null;
				int? height = (oembedJson["height"] != null	 && oembedJson["height"].Type == JTokenType.Integer) ? (int?)oembedJson["height"] : null;
				String html = (string)oembedJson["html"];
				return new Embed(type, provider, url, width, height, html, oembedJson);
			}

		}

		public interface Slice : Fragment
		{
			string SliceType { get; }
			string SliceLabel { get; }

			string AsHtml(DocumentLinkResolver resolver);
		}

		public class SimpleSlice : Slice
		{
			private string sliceType;
			public string SliceType {
				get { return sliceType; }
			}
			private string sliceLabel;
			public string SliceLabel {
				get { return sliceLabel; }
			}
			private Fragment value;
			public Fragment Value {
				get { return value; }
			}

			public SimpleSlice(string sliceType, string sliceLabel, Fragment value) {
				this.sliceType = sliceType;
				this.sliceLabel = sliceLabel;
				this.value = value;
			}

			public string AsHtml(DocumentLinkResolver resolver) {
				var className = "slice";
				if (this.sliceLabel != null) className += (" " + this.sliceLabel);
				return "<div data-slicetype=\"" + this.sliceType + "\" class=\"" + className + "\">" +
					   WithFragments.GetHtml(this.value, resolver, null) +
					   "</div>";
			}

		}

		public class CompositeSlice : Slice
		{
			private readonly Group _repeat;
			private readonly GroupDoc _nonRepeat;

			public string SliceType { get; }

			public string SliceLabel { get; }

			public GroupDoc GetPrimary() {
				return _nonRepeat;
			}

			public Group GetItems() {
				return _repeat;
			}

			public string Items {
				get { return SliceLabel; }
			}

			public CompositeSlice(string sliceType, string label, Group repeat, GroupDoc nonRepeat) {
				SliceType = sliceType;
				SliceLabel = label;
				_repeat = repeat;
				_nonRepeat = nonRepeat;
			}

			public string AsHtml(DocumentLinkResolver resolver) {
				String className = "slice";
				if (SliceLabel != null && SliceLabel != "null")
					className += (" " + SliceLabel);

				List<GroupDoc> groupDocs = new List<GroupDoc> { _nonRepeat };
				return "<div data-slicetype=\"" + SliceType + "\" class=\"" + className + "\">" +
							WithFragments.GetHtml(new Group(groupDocs), resolver, null) +
							WithFragments.GetHtml(_repeat, resolver, null) +
					   "</div>";
			}
		}

		public class SliceZone : Fragment {
			private IList<Slice> slices;
			public IList<Slice> Slices
			{
				get { return slices; }
			}

			public SliceZone(IList<Slice> slices) {
				this.slices = slices;
			}

			public String AsHtml(DocumentLinkResolver linkResolver) {
				return slices.Aggregate("", (html, slice) => html + slice.AsHtml(linkResolver));
			}

			public static SliceZone Parse(JToken json) {
				var slices = new List<Slice>();
				foreach (JToken sliceJson in (JArray)json)
				{
					String sliceType = (string)sliceJson["slice_type"];
					String label = (string)sliceJson["slice_label"];

					// Handle Deprecated SliceZones
					JToken fragJson = sliceJson["value"];
					if (fragJson != null)
					{
						string fragmentType = (string)fragJson["type"];
						JToken fragmentValue = fragJson["value"];
						Fragment value = FragmentParser.Parse(fragmentType, fragmentValue);
						slices.Add(new SimpleSlice(sliceType, label, value));
					}
					else {
						//Parse new format non-repeating slice zones
						JObject nonRepeatsJson = (JObject)sliceJson["non-repeat"];
						GroupDoc nonRepeat = GroupDoc.Parse(nonRepeatsJson);
						JArray repeatJson = (JArray)sliceJson["repeat"];
						Group repeat = Group.Parse(repeatJson);
						slices.Add(new CompositeSlice(sliceType, label, repeat, nonRepeat));
					}
				}
				return new SliceZone(slices);
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

			public static Group Parse(JToken json) {
				var groupDocs = new List<GroupDoc> ();
				foreach (JToken groupJson in (JArray)json) {
					// each groupJson looks like this: { "somelink" : { "type" : "Link.document", { ... } }, "someimage" : { ... } }
					var fragmentMap = new Dictionary<String, Fragment>();
					foreach (KeyValuePair<String, JToken> field in (JObject)groupJson) {
						String fragmentType = (string)field.Value["type"];
						JToken fragmentValue = field.Value["value"];
						Fragment fragment = FragmentParser.Parse(fragmentType, fragmentValue);
						if (fragment != null) fragmentMap[field.Key] = fragment;
					}
					groupDocs.Add(new GroupDoc(fragmentMap));
				}
				return new Group(groupDocs);
			}

		}

		public class Color: Fragment {
			private String hex;
			public String Hex {
				get { return hex; }
			}

			public Color(String hex) {
				this.hex = hex;
			}

			public String AsHtml() {
				return ("<span class=\"color\">" + hex + "</span>");
			}

			private static Regex r = new Regex (@"#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})");

			public static Color Parse(JToken json) {
				String hex = (string)json;
				if (r.Match(hex).Success) {
					return new Color(hex);
				}
				return null;
			}

		}

		public class GeoPoint: Fragment {
			private Double latitude;
			private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");

			public Double Latitude {
				get { return latitude; }
			}
			private Double longitude;
			public Double Longitude {
				get { return longitude; }
			}

			public GeoPoint(Double latitude, Double longitude) {
				this.latitude = latitude;
				this.longitude = longitude;
			}

			public static GeoPoint Parse(JToken json) {
				try {
					Double latitude = Double.Parse((string)json["latitude"], _defaultCultureInfo);
					Double longitude = Double.Parse((string) json["longitude"], _defaultCultureInfo);
					return new GeoPoint(latitude, longitude);
				} catch(Exception) {
					return null;
				}
			}
		}

		public static class FragmentParser {
			public static Fragment Parse(String type, JToken json) {
				switch (type) {
				case "StructuredText":
					return StructuredText.Parse (json);
				case "Image":
					return Image.Parse (json);
				case "Link.web":
					return WebLink.Parse (json);
				case "Link.document":
					return DocumentLink.Parse (json);
				case "Link.file":
					return FileLink.Parse (json);
				case "Link.image":
					return ImageLink.Parse (json);
				case "Text":
					return Text.Parse (json);
				case "Select":
					return Text.Parse (json);
				case "Date":
					return Date.Parse (json);
				case "Timestamp":
					return Timestamp.Parse (json);
				case "Number":
					return Number.Parse (json);
				case "Color":
					return Color.Parse (json);
				case "Embed":
					return Embed.Parse (json);
				case "GeoPoint":
					return GeoPoint.Parse (json);
				case "Group":
					return Group.Parse(json);
				case "SliceZone":
					return SliceZone.Parse(json);
				default:
					return null;
				}
		}

	}

	}
}
