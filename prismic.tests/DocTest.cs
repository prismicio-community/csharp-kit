using NUnit.Framework;
using prismic;
using System;
using System.Linq;
using System.ComponentModel;
using Newtonsoft.Json;

namespace prismic.tests
{
	[TestFixture ()]
	public class DocTest
	{
		[Test ()]
		public void ApiTest ()
		{
			// startgist:c023234afbc20303f792:prismic-api.cs
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			// endgist
			Assert.IsNotNull (api);
		}

		[Test ()]
		[ExpectedException(typeof(AggregateException))]
		public void PrivateApiTest ()
		{
			// startgist:a6f1067b28cc9dca7a82:prismic-apiPrivate.cs
			// This will fail because the token is invalid, but this is how to access a private API
			Api api = prismic.Api.Get("MC5-XXXXXXX-vRfvv70", "https://lesbonneschoses.prismic.io/api");
			// endgist
		}

		[Test ()]
		public void ReferencesTest ()
		{
			// startgist:7b8defb1e1057ad27494:prismic-references.cs
			var previewToken = "MC5VbDdXQmtuTTB6Z0hNWHF3.c--_vVbvv73vv73vv73vv71EA--_vS_vv73vv70T77-9Ke-_ve-_vWfvv70ebO-_ve-_ve-_vQN377-9ce-_vRfvv70";
			Api api = prismic.Api.Get(previewToken, "https://lesbonneschoses.prismic.io/api");
			var stPatrickRef = api.Ref("St-Patrick specials");
			// Now we'll use this reference for all our calls
			var response = api.Form("everything")
				.Ref(stPatrickRef)
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.Submit();
			// The documents object contains a Response object with all documents of type "product"
			// including the new "Saint-Patrick's Cupcake"
			// endgist
			Assert.AreEqual(17, response.Results.Count());
		}

		[Test ()]
		public void SimpleQueryTest ()
		{
			// startgist:6b01f5bd50568045f9a0:prismic-simplequery.cs
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.Submit();
			// The response object contains all documents of type "product", paginated
			// endgist
			Assert.AreEqual (16, response.Results.Count());
		}

		[Test ()]
		public void OrderingsTest ()
		{
			// startgist:6437bcf0207f170dace9:prismic-orderings.cs
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.PageSize(100)
				.Orderings("[my.product.price desc]")
				.Submit();
			// The products are now ordered by price, highest first
			var results = response.Results;
			// endgist
			Assert.AreEqual(100, response.ResultsPerPage);
		}

		[Test ()]
		public void PredicatesTest ()
		{
			// startgist:dbd1a1f4056ae7bf9959:prismic-predicates.cs
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.type, ""blog-post"")][:d = date.after(my.blog-post.date, 1401580800000)]]")
				.Submit();
			// endgist
			Assert.AreEqual (0, response.Results.Count());
		}

		[Test ()]
		public void AllPredicatesTest ()
		{
			// startgist:26e651e93de58bdf7165:prismic-allPredicates.cs
			// "at" predicate: equality of a fragment to a value.
			var at = "[[:d = at(document.type, \"article\")]]";
			// "any" predicate: equality of a fragment to a value.
			var any = "[[:d = any(document.type, [\"article\", \"blog-post\"])]]";

			// "fulltext" predicate: fulltext search in a fragment.
			var fulltext = "[[:d = fulltext(my.article.body, \"sausage\")]]";

			// "similar" predicate, with a document id as reference
			var similar = "[[:d = similar(\"UXasdFwe42D\", 10)]]";
			// endgist
		}

		[Test ()]
		public void GetTextTest ()
		{
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbl"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:7869828eaa8c1b8555d3:prismic-getText.cs
			var author = doc.GetText("blog-post.author").Value;
			// endgist
			Assert.AreEqual(author, "John M. Martelle, Fine Pastry Magazine"); // gisthide
		}

		[Test ()]
		public void GetNumberTest()
		{
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:57e8cda4c83cadf7f7d0:prismic-getNumber.cs
			// Number predicates
			var gt = "[[:d = number.gt(my.product.price, 10)]]";
			var lt = "[[:d = number.lt(my.product.price, 20)]]";
			var inRange = "[[:d = number.inRange(my.product.price, 10, 20)]]";

			// Accessing number fields
			var price = doc.GetNumber("product.price").Value;
			// endgist
			Assert.AreEqual(price, 2.5);
		}

		[Test ()]
		public void ImagesTest()
		{
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:2ba6c72a80cf9d2af15e:prismic-images.cs
			// Accessing image fields
			var imageView = doc.GetImageView("product.image", "main");
			String url = imageView.Url;
			// endgist
			Assert.AreEqual(url, "https://prismic-io.s3.amazonaws.com/lesbonneschoses/f606ad513fcc2a73b909817119b84d6fd0d61a6d.png");
		}

		[Test ()]
		public void DateTimestampTest()
		{
			Api api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbl"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:653bcba6211a9b71429d:prismic-dateTimestamp.cs
			// Date and Timestamp predicates
			var dateBefore = "[[:d = date.before(my.product.releaseDate, \"2014-6-1\")]]";
			var dateAfter = "[[:d = date.after(my.product.releaseDate, \"2014-1-1\")]]";
			var dateBetween = "[[:d = date.between(my.product.releaseDate, \"2014-1-1\", \"2014-6-1\")]]";
			var dayOfMonth = "[[:d = date.day-of-month(my.product.releaseDate, 14)]]";
			var dayOfMonthAfter = "[[:d = date.day-of-month-after(my.product.releaseDate, 14)]]";
			var dayOfMonthBefore = "[[:d = date.day-of-month-before(my.product.releaseDate, 14)]]";
			var dayOfWeek = "[[:d = date.day-of-week(my.product.releaseDate, \"Tuesday\")]]";
			var dayOfWeekAfter = "[[:d = date.day-of-week-after(my.product.releaseDate, \"Wednesday\")]]";
			var dayOfWeekBefore = "[[:d = date.day-of-week-before(my.product.releaseDate, \"Wednesday\")]]";
			var month = "[[:d = date.month(my.product.releaseDate, \"June\")]]";
			var monthBefore = "[[:d = date.month-before(my.product.releaseDate, \"June\")]]";
			var monthAfter = "[[:d = date.month-after(my.product.releaseDate, \"June\")]]";
			var year = "[[:d = date.year(my.product.releaseDate, 2014)]]";
			var hour = "[[:d = date.hour(my.product.releaseDate, 12)]]";
			var hourBefore = "[[:d = date.hour-before(my.product.releaseDate, 12)]]";
			var hourAfter = "[[:d = date.hour-after(my.product.releaseDate, 12)]]";

			// Accessing Date and Timestamp fields
			var date = doc.GetDate("blog-post.date").Value;
			var dateYear = date.Year;
			var updateTime = doc.GetTimestamp("blog-post.update");
			if (updateTime != null) {
				var updateHour = updateTime.Value.Hour;
			}
			// endgist
			Assert.AreEqual(dateYear, 2013);
		}

		[Test ()]
		public void GroupTest()
		{
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));

			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"documents\":{\"type\":\"Group\",\"value\":[{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDejAEAAFwMyrW9\",\"type\":\"doc\",\"tags\":[],\"slug\":\"installing-meta-micro\"},\"isBroken\":false}},\"desc\":{\"type\":\"StructuredText\",\"value\":[{\"type\":\"paragraph\",\"text\":\"A detailed step by step point of view on how installing happens.\",\"spans\":[]}]}},{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDmKgEAALwMyrXA\",\"type\":\"doc\",\"tags\":[],\"slug\":\"using-meta-micro\"},\"isBroken\":false}}}]}}}}";
			var document = JsonConvert.DeserializeObject<Document>(json);
			// startgist:b5b40c40a696911081d7:prismic-group.cs
			var group = document.GetGroup("article.documents");
			foreach (GroupDoc doc in group.GroupDocs) {
				// Desc and Link are Fragments, their type depending on what's declared in the Document Mask
				try {
					var desc = doc.GetStructuredText["desc"];
					var link = doc.GetLink["linktodoc"];
				} catch (Exception e) {
					// Missing key
				}
			}
			// endgist
			var firstDesc = (prismic.Fragments.Fragment.StructuredText)group.Item.First().fragments["desc"];
			Assert.AreEqual(firstDesc.AsHtml(resolver), "<p>A detailed step by step point of view on how installing happens.</p>");
		}

		[Test ()]
		public void LinkTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"source\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UlfoxUnM0wkXYXbE\",\"type\":\"product\",\"tags\":[\"Macaron\"],\"slug\":\"dark-chocolate-macaron\"},\"isBroken\":false}}}}}";
			var document = Api.Document.FromJson(FSharp.Data.JsonValue.Parse(json, null));
			// startgist:f439d465a87cbddb2737:prismic-link.cs
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Id, l.Slug));
			var source = document.GetLink("article.source").Value();
			var url = source.GetUrl(resolver);
			// endgist
			Assert.AreEqual("http://localhost/UlfoxUnM0wkXYXbE/dark-chocolate-macaron", url);
		}

		[Test ()]
		public void EmbedTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"video\":{\"type\":\"Embed\",\"value\":{\"oembed\":{\"provider_url\":\"http://www.youtube.com/\",\"type\":\"video\",\"thumbnail_height\":360,\"height\":270,\"thumbnail_url\":\"http://i1.ytimg.com/vi/baGfM6dBzs8/hqdefault.jpg\",\"width\":480,\"provider_name\":\"YouTube\",\"html\":\"<iframe width=\\\"480\\\" height=\\\"270\\\" src=\\\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\\\" frameborder=\\\"0\\\" allowfullscreen></iframe>\",\"author_name\":\"Siobhan Wilson\",\"version\":\"1.0\",\"author_url\":\"http://www.youtube.com/user/siobhanwilsonsongs\",\"thumbnail_width\":480,\"title\":\"Siobhan Wilson - All Dressed Up\",\"embed_url\":\"https://www.youtube.com/watch?v=baGfM6dBzs8\"}}}}}}";
			var document = JsonConvert.DeserializeObject<Document>(json);
			// startgist:a0a1846d443b2fa39097:prismic-embed.cs
			var video = document.GetEmbed ("article.video");
			// Html is the code to include to embed the object, and depends on the embedded service
			var html = video.Value().Item.html.Value();
			// endgist
			Assert.AreEqual("<iframe width=\"480\" height=\"270\" src=\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\" frameborder=\"0\" allowfullscreen></iframe>", html);
		}

		[Test()]
		public void ColorTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"background\":{\"type\":\"Color\",\"value\":\"#000000\"}}}}";
			var document = Api.Document.fromJson(FSharp.Data.JsonValue.Parse(json, null));
			// startgist:0d0fa9849ae5ff7c921c:prismic-color.cs
			var bgcolor = document.GetColor("article.background");
			var hex = bgcolor.Value ().Item.hex;
			// endgist
			Assert.AreEqual("#000000", hex);
		}

		[Test()]
		public void GeopointTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"location\":{\"type\":\"GeoPoint\",\"value\":{\"latitude\":48.877108,\"longitude\":2.333879}}}}}";
			var document = Api.Document.fromJson(FSharp.Data.JsonValue.Parse(json, null));
			// startgist:ffd5197f8b1f3c9b302c:prismic-geopoint.cs
			// "near" predicate for GeoPoint fragments
			var near = "[[:d = geopoint.near(my.store.location, 48.8768767, 2.3338802, 10)]]";

			// Accessing GeoPoint fragments
			var place = document.GetGeoPoint("article.location").Value().Item;
			var coordinates = place.latitude + "," + place.longitude;
			// endgist
			Assert.AreEqual(coordinates, "48.877108,2.333879");
		}

		[Test ()]
		public void AsHtmlTest ()
		{
			var api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api
				.Forms["everything"]
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]")
				.SubmitableAsTask().Submit().Result;
			// startgist:097067bd2495233520bb:prismic-asHtml.cs
			var document = response.results.First ();
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var html = document.GetStructuredText ("blog-post.body").BindAsHtml(resolver);
			// endgist
			Assert.IsTrue (html.Exists ());
		}

		[Test ()]
		public void HtmlSerializerTest ()
		{
			var api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api");
			var response = api
				.Forms["everything"]
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]")
				.SubmitableAsTask().Submit().Result;
			// startgist:b5f2de0fb813b52a14a9:prismic-htmlSerializer.cs
			var document = response.results.First ();
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var serializer = prismic.HtmlSerializer.For ((elt, body) => {
				if (elt is Fragments.Span.Hyperlink) {
					// Add a class to hyperlinks
					var link = ((Fragments.Span.Hyperlink)elt).Item.Item3;
					if (link is Fragments.Link.DocumentLink) {
						var doclink = ((Fragments.Link.DocumentLink)link).Item;
						return String.Format("<a class=\"some-link\" href=\"{0}\">{1}</a>", resolver.Apply(doclink), body);
					}
				}
				if (elt is Fragments.Block.Image) {
					// Don't wrap images in <p> blocks
					var imageview = ((Fragments.Block.Image)elt).Item;
					return imageview.AsHtml(resolver);
				}
				return null;
			});
			var html = document.GetStructuredText ("blog-post.body").BindAsHtml(resolver, serializer);
			// endgist
			Assert.IsTrue (html.Exists ());
		}

		[Test()]
		public void CacheTest()
		{
			// startgist:9307922348c5ce1ef34c:prismic-cache.cs
			var cache = prismic.Cache.For<Api.Response> (
				(key, value, ttl) => {
					return null;
				},
				key => {
					return null;
				}
			);
			// This Api will use the custom cache object
			var api = prismic.Api.Get("https://lesbonneschoses.prismic.io/api", cache, null);
			// endgist
		}

	}


}
