using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace prismic.tests
{
    [TestClass]
	public class DocTest
    {
        private readonly string ApiUrl = "https://lesbonneschoses.cdn.prismic.io/api";

        [TestMethod]
		public async Task ApiTest ()
		{
			// startgist:30e5810c1c9c50a37f39:prismic-api.cs
			// Fetching the API is an asynchronous process
			Api api = await Api.Get(ApiUrl);
			// endgist
			Assert.IsNotNull (api);
		}

		[TestMethod]
		public void PrivateApiTest()
		{
			// startgist:cc56a498cac2ba43d96c:prismic-apiPrivate.cs
			// This will fail because the token is invalid, but this is how to access a private API
			Assert.ThrowsExceptionAsync<AggregateException>(()=>Api.Get(ApiUrl, "MC5-XXXXXXX-vRfvv70"));
			// endgist
		}

		[TestMethod]
		public async Task ReferencesTest ()
		{
			// startgist:ce58be224dda1a3c080a:prismic-references.cs
			var previewToken = "MC5VbDdXQmtuTTB6Z0hNWHF3.c--_vVbvv73vv73vv73vv71EA--_vS_vv73vv70T77-9Ke-_ve-_vWfvv70ebO-_ve-_ve-_vQN377-9ce-_vRfvv70";
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api", previewToken);
			var stPatrickRef = api.Ref("St-Patrick specials");
			// Now we'll use this reference for all our calls
			Response response = await api
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.Ref(stPatrickRef)
				.Submit();
			// The documents object contains a Response object with all documents of type "product"
			// including the new "Saint-Patrick's Cupcake"
			// endgist
			Assert.AreEqual(17, response.Results.Count());
		}

		[TestMethod]
		public async Task SimpleQueryTest ()
		{
			// startgist:e3a75d3b157ed11e60ff:prismic-simplequery.cs
			Api api = await prismic.Api.Get(ApiUrl);
			// Just like Api.Get, fetching a Response is asynchronous
			Response response = await api
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.Submit();
			// The response object contains all documents of type "product", paginated
			// endgist
			Assert.AreEqual (16, response.Results.Count());
		}

		[TestMethod]
		public async Task OrderingsTest ()
		{
			// startgist:2835cc08041b530da0e3:prismic-orderings.cs
			Api api = await prismic.Api.Get("https://lesbonneschoses.cdn.prismic.io/api");
			var response = await api
				.Query (@"[[:d = at(document.type, ""product"")]]")
				.PageSize(100)
				.Orderings("[my.product.price desc]")
				.Submit();
			// The products are now ordered by price, highest first
			var results = response.Results;
			// endgist
			Assert.AreEqual(100, response.ResultsPerPage);
		}

		[TestMethod]
		public async Task PredicatesTest ()
		{
			// startgist:16caadec7671853d77f0:prismic-predicates.cs
			Api api = await Api.Get(ApiUrl);
			var response = await api
				.Query (Predicates.at("document.type", "blog-post"), Predicates.dateAfter("my.blog-post.date", DateTime.Now))
				.Submit();
			// endgist
			Assert.AreEqual (0, response.Results.Count());
		}

		[TestMethod]
		public void AllPredicatesTest ()
		{
			// startgist:e65ee8392a8b6c8aedc4:prismic-allPredicates.cs
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

		[TestMethod]
		public async Task GetTextTest ()
		{
			Api api = await prismic.Api.Get(ApiUrl);
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbl"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:15c8532139ffcb369b95:prismic-getText.cs
			var author = doc.GetText("blog-post.author");
			// endgist
			Assert.AreEqual(author, "John M. Martelle, Fine Pastry Magazine"); // gisthide
		}

		[TestMethod]
		public async Task GetNumberTest()
		{
			Api api = await prismic.Api.Get(ApiUrl);
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:1a6c8386fd678572d8b0:prismic-getNumber.cs
			// Number predicates
			var gt = Predicates.gt("my.product.price", 10);
			var lt = Predicates.lt("my.product.price", 20);
			var inRange = Predicates.inRange("my.product.price", 10, 20);

			// Accessing number fields
			decimal price = doc.GetNumber("product.price").Value;
			// endgist
			Assert.AreEqual(price, 2.5);
		}

		[TestMethod]
		public async Task ImagesTest()
		{
			Api api = await prismic.Api.Get(ApiUrl);
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbO"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:4e94efd4d09576b05930:prismic-images.cs
			// Accessing image fields
			fragments.Image.View imageView = doc.GetImageView("product.image", "main");
			String url = imageView.Url;
			// endgist
			Assert.AreEqual("https://d2aw36oac6sa9o.cloudfront.net/lesbonneschoses/f606ad513fcc2a73b909817119b84d6fd0d61a6d.png", url);
		}

		[TestMethod]
		public async Task DateTimestampTest()
		{
			Api api = await prismic.Api.Get(ApiUrl);
			Console.WriteLine ("DateTimestampTest, got API " + api);
			var response = await api.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbl"")]]")
				.Submit();
			var doc = response.Results[0];
			// startgist:cc12f51851d59e24c956:prismic-dateTimestamp.cs
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
			Assert.AreEqual(dateYear, 2013);
		}

		[TestMethod]
		public void GroupTest()
		{
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));

			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"documents\":{\"type\":\"Group\",\"value\":[{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDejAEAAFwMyrW9\",\"type\":\"doc\",\"tags\":[],\"slug\":\"installing-meta-micro\"},\"isBroken\":false}},\"desc\":{\"type\":\"StructuredText\",\"value\":[{\"type\":\"paragraph\",\"text\":\"A detailed step by step point of view on how installing happens.\",\"spans\":[]}]}},{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDmKgEAALwMyrXA\",\"type\":\"doc\",\"tags\":[],\"slug\":\"using-meta-micro\"},\"isBroken\":false}}}]}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:5926b0f6454f25e70350:prismic-group.cs
			var group = document.GetGroup("article.documents");
			foreach (GroupDoc doc in group.GroupDocs) {
				try {
					fragments.StructuredText desc = doc.GetStructuredText("desc");
					fragments.Link link = doc.GetLink("linktodoc");
				} catch (Exception) {
					// Missing key
				}
			}
			// endgist
			var firstDesc = group.GroupDocs [0].GetStructuredText ("desc");
			Assert.AreEqual(firstDesc.AsHtml(resolver), "<p>A detailed step by step point of view on how installing happens.</p>");
		}

		[TestMethod]
		public void LinkTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"source\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UlfoxUnM0wkXYXbE\",\"type\":\"product\",\"tags\":[\"Macaron\"],\"slug\":\"dark-chocolate-macaron\"},\"isBroken\":false}}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:ef7313f73b0a9488fb47:prismic-link.cs
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Id, l.Slug));
			var source = document.GetLink("article.source");
			var url = source.GetUrl(resolver);
			// endgist
			Assert.AreEqual("http://localhost/UlfoxUnM0wkXYXbE/dark-chocolate-macaron", url);
		}

		[TestMethod]
		public void EmbedTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"video\":{\"type\":\"Embed\",\"value\":{\"oembed\":{\"provider_url\":\"http://www.youtube.com/\",\"type\":\"video\",\"thumbnail_height\":360,\"height\":270,\"thumbnail_url\":\"http://i1.ytimg.com/vi/baGfM6dBzs8/hqdefault.jpg\",\"width\":480,\"provider_name\":\"YouTube\",\"html\":\"<iframe width=\\\"480\\\" height=\\\"270\\\" src=\\\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\\\" frameborder=\\\"0\\\" allowfullscreen></iframe>\",\"author_name\":\"Siobhan Wilson\",\"version\":\"1.0\",\"author_url\":\"http://www.youtube.com/user/siobhanwilsonsongs\",\"thumbnail_width\":480,\"title\":\"Siobhan Wilson - All Dressed Up\",\"embed_url\":\"https://www.youtube.com/watch?v=baGfM6dBzs8\"}}}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:dabf36e591c93029440a:prismic-embed.cs
			var video = document.GetEmbed ("article.video");
			// Html is the code to include to embed the object, and depends on the embedded service
			var html = video.Html;
			// endgist
			Assert.AreEqual("<iframe width=\"480\" height=\"270\" src=\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\" frameborder=\"0\" allowfullscreen></iframe>", html);
		}

		[TestMethod]
		public void ColorTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"background\":{\"type\":\"Color\",\"value\":\"#000000\"}}}}";
			var document = Document.Parse(JObject.Parse(json));
			// startgist:87cc65a8d1d02f4b4342:prismic-color.cs
			var bgcolor = document.GetColor("article.background");
			var hex = bgcolor.Hex;
			// endgist
			Assert.AreEqual("#000000", hex);
		}

		[TestMethod]
		public void GeopointTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"location\":{\"type\":\"GeoPoint\",\"value\":{\"latitude\":48.877108,\"longitude\":2.333879}}}}}";
			var document = Document.Parse(JObject.Parse(json));

#pragma warning disable CS0219 // Variable is assigned but its value is never used
            // startgist:afd2b8109ce21af4564c:prismic-geopoint.cs
            // "near" predicate for GeoPoint fragments
            var near = "[[:d = geopoint.near(my.store.location, 48.8768767, 2.3338802, 10)]]";

			// Accessing GeoPoint fragments
			fragments.GeoPoint place = document.GetGeoPoint("article.location");
			var coordinates = place.Latitude + "," + place.Longitude;
            // endgist
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            Assert.AreEqual(place.Latitude, 48.877108);
            Assert.AreEqual(place.Longitude, 2.333879);
		}

		[TestMethod]
		public async Task AsHtmlTest ()
		{
			var api = await prismic.Api.Get(ApiUrl);
			var response = await api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]")
				.Submit();
			// startgist:90c0de35cd9a363bb60b:prismic-asHtml.cs
			var document = response.Results.First ();
			var resolver =
                DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var html = document.GetStructuredText ("blog-post.body").AsHtml(resolver);
			// endgist
			Assert.IsNotNull (html);
		}

		[TestMethod]
		public async Task HtmlSerializerTest ()
		{
			var api = await prismic.Api.Get(ApiUrl);
			var response = await api
				.Form("everything")
				.Ref(api.Master)
				.Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]")
				.Submit();
			// startgist:99a0a66b6dfe2b9ce78c:prismic-htmlSerializer.cs
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

		[TestMethod]
		public void CacheTest()
		{
			// startgist:9711e4670aaa97c975c8:prismic-cache.cs
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
