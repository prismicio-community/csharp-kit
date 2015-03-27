using NUnit.Framework;
using prismic;
using System;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace prismic.tests
{
	[TestFixture ()]
	public class DocTest
	{
		[Test ()]
		public async Task ApiTest ()
		{
			// startgist:c023234afbc20303f792:prismic-api.cs
			// Fetching the API is an asynchronous process
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			// endgist
			Assert.IsNotNull (api);
		}

		[Test ()]
		[ExpectedException(typeof(AggregateException))]
        public void PrivateApiTest()
		{
			// startgist:a6f1067b28cc9dca7a82:prismic-apiPrivate.cs
			// This will fail because the token is invalid, but this is how to access a private API
			Api api = prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api", "MC5-XXXXXXX-vRfvv70").Result;
			// endgist
		}

		[Test ()]
		public async Task ReferencesTest ()
		{
			// startgist:7b8defb1e1057ad27494:prismic-references.cs
			var previewToken = "MC5VbDdXQmtuTTB6Z0hNWHF3.c--_vVbvv73vv73vv73vv71EA--_vS_vv73vv70T77-9Ke-_ve-_vWfvv70ebO-_ve-_ve-_vQN377-9ce-_vRfvv70";
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api", previewToken);
			Console.WriteLine ("API OK");
			var stPatrickRef = api.Ref("St-Patrick specials");
			Console.WriteLine ("StPar = " + stPatrickRef);
			// Now we'll use this reference for all our calls
			Response response = await api.Form("everything")
				.Ref(stPatrickRef)
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.Submit();
			// The documents object contains a Response object with all documents of type "product"
			// including the new "Saint-Patrick's Cupcake"
			// endgist
			Assert.AreEqual(17, response.Results.Count());
		}

		[Test ()]
		public async Task SimpleQueryTest ()
		{
			// startgist:6b01f5bd50568045f9a0:prismic-simplequery.cs
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			// Just like Api.Get, fetching a Response is asynchronous
			Response response = await api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.Submit();
			// The response object contains all documents of type "product", paginated
			// endgist
			Assert.AreEqual (16, response.Results.Count());
		}

		[Test ()]
		public async Task OrderingsTest ()
		{
			// startgist:6437bcf0207f170dace9:prismic-orderings.cs
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api.Form("everything")
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
		public async Task PredicatesTest ()
		{
			// startgist:dbd1a1f4056ae7bf9959:prismic-predicates.cs
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api
				.Form("everything")
				.Ref(api.Master)
				.Query (Predicates.at("document.type", "blog-post"), Predicates.dateAfter("my.blog-post.date", DateTime.Now))
				.Submit();
			// endgist
			Assert.AreEqual (0, response.Results.Count());
		}

		[Test ()]
		public void AllPredicatesTest ()
		{
			// startgist:26e651e93de58bdf7165:prismic-allPredicates.cs
			// "at" predicate: equality of a fragment to a value.
			var at = Predicates.at("document.type", "article");
			// "any" predicate: equality of a fragment to a value.
			var any = Predicates.any("document.type", new string[] {"article", "blog-post"});

			// "fulltext" predicate: fulltext search in a fragment.
			var fulltext = Predicates.fulltext("my.article.body", "sausage");

			// "similar" predicate, with a document id as reference
			var similar = Predicates.similar("UXasdFwe42D", 10);
			// endgist
		}

		[Test ()]
		public async Task GetTextTest ()
		{
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbl"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:7869828eaa8c1b8555d3:prismic-getText.cs
			var author = doc.GetText("blog-post.author");
			// endgist
			Assert.AreEqual("John M. Martelle, Fine Pastry Magazine", author); // gisthide
		}

		[Test ()]
		public async Task GetNumberTest()
		{
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:57e8cda4c83cadf7f7d0:prismic-getNumber.cs
			// Number predicates
			var gt = Predicates.gt("my.product.price", 10);
			var lt = Predicates.lt("my.product.price", 20);
			var inRange = Predicates.inRange("my.product.price", 10, 20);

			// Accessing number fields
			double price = doc.GetNumber("product.price", new CultureInfo("en-US")).Value;
			// endgist
            Assert.AreEqual(2.5, price);
		}

        [Test()]
        public async Task GetDecimalTest()
        {
            Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
            var response = await api.Form("everything")
                .Ref(api.Master)
                .Query(@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
                .Submit();
            var doc = response.Results[0];
            
            decimal price = doc.GetDecimal("product.price", new CultureInfo("en-US")).Value;

            Assert.AreEqual(2.5, price);
        }

		[Test ()]
		public async Task ImagesTest()
		{
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:2ba6c72a80cf9d2af15e:prismic-images.cs
			// Accessing image fields
			fragments.Image.View imageView = doc.GetImageView("product.image", "main");
			String url = imageView.Url;
			// endgist
            Assert.AreEqual("https://lesbonneschoses.cdn.prismic.io/lesbonneschoses/f606ad513fcc2a73b909817119b84d6fd0d61a6d.png", url);
		}

		[Test ()]
		public async Task DateTimestampTest()
		{
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			Console.WriteLine ("DateTimestampTest, got API " + api);
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbl"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:653bcba6211a9b71429d:prismic-dateTimestamp.cs
			// Date and Timestamp predicates
			var dateBefore = Predicates.dateBefore("my.product.releaseDate", new DateTime(2014, 6, 1));
			var dateAfter = Predicates.dateAfter("my.product.releaseDate", new DateTime(2014, 1, 1));
			var dateBetween = Predicates.dateBetween("my.product.releaseDate", new DateTime(2014, 1, 1), new DateTime(2014, 6, 1));
			var dayOfMonth = Predicates.dayOfMonth("my.product.releaseDate", 14);
			var dayOfMonthAfter = Predicates.dayOfMonthAfter("my.product.releaseDate", 14);
			var dayOfMonthBefore = Predicates.dayOfMonthBefore("my.product.releaseDate", 14);
			var dayOfWeek = Predicates.dayOfWeek("my.product.releaseDate", DayOfWeek.Tuesday);
			var dayOfWeekAfter = Predicates.dayOfWeekAfter("my.product.releaseDate", DayOfWeek.Wednesday);
			var dayOfWeekBefore = Predicates.dayOfWeekBefore("my.product.releaseDate", DayOfWeek.Wednesday);
			var month = Predicates.month("my.product.releaseDate", Predicates.Month.June);
			var monthBefore = Predicates.monthBefore("my.product.releaseDate", Predicates.Month.June);
			var monthAfter = Predicates.monthAfter("my.product.releaseDate", Predicates.Month.June);
			var year = Predicates.year("my.product.releaseDate", 2014);
			var hour = Predicates.hour("my.product.releaseDate", 12);
			var hourBefore = Predicates.hourBefore("my.product.releaseDate", 12);
			var hourAfter = Predicates.hourAfter("my.product.releaseDate", 12);

			// Accessing Date and Timestamp fields
			DateTime date = doc.GetDate("blog-post.date").Value;
			int dateYear = date.Year;
			fragments.Timestamp updateTime = doc.GetTimestamp("blog-post.update");
			if (updateTime != null) {
				int updateHour = updateTime.Value.Hour;
			}
			// endgist
            Assert.AreEqual(2013, dateYear);
		}

		[Test ()]
		public void GroupTest()
		{
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));

			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"documents\":{\"type\":\"Group\",\"value\":[{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDejAEAAFwMyrW9\",\"type\":\"doc\",\"tags\":[],\"slug\":\"installing-meta-micro\"},\"isBroken\":false}},\"desc\":{\"type\":\"StructuredText\",\"value\":[{\"type\":\"paragraph\",\"text\":\"A detailed step by step point of view on how installing happens.\",\"spans\":[]}]}},{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDmKgEAALwMyrXA\",\"type\":\"doc\",\"tags\":[],\"slug\":\"using-meta-micro\"},\"isBroken\":false}}}]}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:b5b40c40a696911081d7:prismic-group.cs
			var group = document.GetGroup("article.documents");
			foreach (GroupDoc doc in group.GroupDocs) {
				try {
					fragments.StructuredText desc = doc.GetStructuredText("desc");
					fragments.Link link = doc.GetLink("linktodoc");
				} catch (Exception e) {
					// Missing key
				}
			}
			// endgist
			var firstDesc = group.GroupDocs [0].GetStructuredText ("desc");
            Assert.AreEqual("<p>A detailed step by step point of view on how installing happens.</p>", firstDesc.AsHtml(resolver));
		}

		[Test ()]
		public void LinkTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"source\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UlfoxUnM0wkXYXbE\",\"type\":\"product\",\"tags\":[\"Macaron\"],\"slug\":\"dark-chocolate-macaron\"},\"isBroken\":false}}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:f439d465a87cbddb2737:prismic-link.cs
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Id, l.Slug));
			var source = document.GetLink("article.source");
			var url = source.GetUrl(resolver);
			// endgist
			Assert.AreEqual("http://localhost/UlfoxUnM0wkXYXbE/dark-chocolate-macaron", url);
		}

		[Test ()]
		public void EmbedTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"video\":{\"type\":\"Embed\",\"value\":{\"oembed\":{\"provider_url\":\"http://www.youtube.com/\",\"type\":\"video\",\"thumbnail_height\":360,\"height\":270,\"thumbnail_url\":\"http://i1.ytimg.com/vi/baGfM6dBzs8/hqdefault.jpg\",\"width\":480,\"provider_name\":\"YouTube\",\"html\":\"<iframe width=\\\"480\\\" height=\\\"270\\\" src=\\\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\\\" frameborder=\\\"0\\\" allowfullscreen></iframe>\",\"author_name\":\"Siobhan Wilson\",\"version\":\"1.0\",\"author_url\":\"http://www.youtube.com/user/siobhanwilsonsongs\",\"thumbnail_width\":480,\"title\":\"Siobhan Wilson - All Dressed Up\",\"embed_url\":\"https://www.youtube.com/watch?v=baGfM6dBzs8\"}}}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:a0a1846d443b2fa39097:prismic-embed.cs
			var video = document.GetEmbed ("article.video");
			// Html is the code to include to embed the object, and depends on the embedded service
			var html = video.Html;
			// endgist
			Assert.AreEqual("<iframe width=\"480\" height=\"270\" src=\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\" frameborder=\"0\" allowfullscreen></iframe>", html);
		}

		[Test()]
		public void ColorTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"background\":{\"type\":\"Color\",\"value\":\"#000000\"}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:0d0fa9849ae5ff7c921c:prismic-color.cs
			var bgcolor = document.GetColor("article.background");
			var hex = bgcolor.Hex;
			// endgist
			Assert.AreEqual("#000000", hex);
		}

		[Test()]
		public void GeopointTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"location\":{\"type\":\"GeoPoint\",\"value\":{\"latitude\":48.877108,\"longitude\":2.333879}}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:ffd5197f8b1f3c9b302c:prismic-geopoint.cs
			// "near" predicate for GeoPoint fragments
			var near = "[[:d = geopoint.near(my.store.location, 48.8768767, 2.3338802, 10)]]";

			// Accessing GeoPoint fragments
			fragments.GeoPoint place = document.GetGeoPoint("article.location");
            var coordinates = place.Latitude.ToString(new CultureInfo("en-US")) + "," + place.Longitude.ToString(new CultureInfo("en-US"));
			// endgist
            Assert.AreEqual("48.877108,2.333879", coordinates);
		}

		[Test ()]
		public async Task AsHtmlTest ()
		{
			var api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]")
				.Submit();
			// startgist:097067bd2495233520bb:prismic-asHtml.cs
			var document = response.Results.First ();
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var html = document.GetStructuredText ("blog-post.body").AsHtml(resolver);
			// endgist
			Assert.IsNotNull (html);
		}

		[Test ()]
		public async Task HtmlSerializerTest ()
		{
			var api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]")
				.Submit();
			// startgist:b5f2de0fb813b52a14a9:prismic-htmlSerializer.cs
			var document = response.Results.First ();
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var serializer = prismic.HtmlSerializer.For ((elt, body) => {
				if (elt is fragments.StructuredText.Hyperlink) {
					var link = ((fragments.StructuredText.Hyperlink)elt).Link;
					// Add a class to hyperlinks
					if (link is fragments.DocumentLink) {
						var doclink = ((fragments.DocumentLink)link);
						return String.Format("<a class=\"some-link\" href=\"{0}\">{1}</a>", resolver.Resolve(doclink), body);
					}
				}
				if (elt is fragments.StructuredText.Image) {
					// Don't wrap images in <p> blocks
					var imageview = ((fragments.StructuredText.Image)elt).View;
					return imageview.AsHtml(resolver);
				}
				return null;
			});
			var html = document.GetStructuredText ("blog-post.body").AsHtml(resolver, serializer);
			// endgist
			Assert.IsNotNull (html);
		}

		[Test()]
		public void CacheTest()
		{
			// startgist:9307922348c5ce1ef34c:prismic-cache.cs
			// TODO
			var cache = LambdaCache.For (
				(key, value, ttl) => {
					return null;
				},
				key => {
					return null;
				}
			);
			// This Api will use the custom cache object
			var api = prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api", cache);
			// endgist
		}

	}


}
