﻿﻿using NUnit.Framework;
using prismic;
using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace prismic.tests
{
	[TestFixture ()]
	public class DocTest
	{
		[Test ()]
		public void DocumentLangTest()
		{
			var document = Fixtures.GetDocument("language.json");
			Assert.AreEqual("de-ch", document.Lang);

			var lang1 = new AlternateLanguage("WZ1iGioAACkA7Kqn", "french", "article", "fr-fr");
			Assert.AreEqual(lang1.Id, document.AlternateLanguages[0].Id);
			Assert.AreEqual(lang1.LANG, document.AlternateLanguages[0].LANG);
			Assert.AreEqual(lang1.TYPE, document.AlternateLanguages[0].TYPE);
			Assert.AreEqual(lang1.UID, document.AlternateLanguages[0].UID);

			var lang2 = new AlternateLanguage("WZ1iPyoAACkA7KtJ", "spanish", "article", "es-es");
			Assert.AreEqual(lang2.Id, document.AlternateLanguages[1].Id);
			Assert.AreEqual(lang2.LANG, document.AlternateLanguages[1].LANG);
			Assert.AreEqual(lang2.TYPE, document.AlternateLanguages[1].TYPE);
			Assert.AreEqual(lang2.UID, document.AlternateLanguages[1].UID);
		}

		[Test ()]
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

		[Test ()]
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
				} catch (Exception e) {
					// Missing key
				}
			}
			// endgist
			var firstDesc = group.GroupDocs [0].GetStructuredText ("desc");
			Assert.AreEqual(firstDesc.AsHtml(resolver), "<p>A detailed step by step point of view on how installing happens.</p>");
		}

		[Test ()]
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

		[Test ()]
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

		[Test()]
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

		[Test()]
		public void GeopointTest()
		{
			var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"location\":{\"type\":\"GeoPoint\",\"value\":{\"latitude\":48.877108,\"longitude\":2.333879}}}}}";
			var document = Document.Parse(JObject.Parse(json));
			fragments.GeoPoint place = document.GetGeoPoint("article.location");
			var coordinates = place.Latitude + "," + place.Longitude;
			// endgist
			Assert.AreEqual(place.Latitude, 48.877108);
			Assert.AreEqual(place.Longitude, 2.333879);
		}
	}
}
