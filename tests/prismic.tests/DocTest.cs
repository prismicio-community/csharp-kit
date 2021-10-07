using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace prismic.AspNetCore.Tests
{


    public class DocTest
    {
        [Fact]
        public void AllPredicatesTest()
        {
#pragma warning disable IDE0059
            // startgist:e65ee8392a8b6c8aedc4:prismic-allPredicates.cs
            // "at" predicate: equality of a fragment to a value.
            var at = Predicates.At("document.type", "article");
            // "any" predicate: equality of a fragment to a value.
            var any = Predicates.Any("document.type", new string[] { "article", "blog-post" });

            // "fulltext" predicate: fulltext search in a fragment.
            var fulltext = Predicates.Fulltext("my.article.body", "sausage");

            // "similar" predicate, with a document id as reference
            var similar = Predicates.Similar("UXasdFwe42D", 10);
            // endgist
        }

        private readonly string DocumentId = @"[[:d = at(document.id, ""XPZFuxAAAIGaEtfj"")]]";

        [Fact]
        public async Task GetTextTest()
        {
            Api api = await TestHelper.GetApi(TestHelper.Endpoint);
            var response = await api.Form("everything")
                .Ref(api.Master)
                .Query(DocumentId)
                .Submit();
            var doc = response.Results[0];
            // startgist:15c8532139ffcb369b95:prismic-getText.cs
            var author = doc.GetText("test_document.text");
            // endgist
            Assert.Equal("Test Document", author); // gisthide
        }

        [Fact]
        public void DocumentLangTest()
        {
            var document = Fixtures.GetDocument("language.json");
            Assert.Equal("de-ch", document.Lang);

            var lang1 = new AlternateLanguage("WZ1iGioAACkA7Kqn", "french", "article", "fr-fr");
            Assert.Equal(lang1.Id, document.AlternateLanguages[0].Id);
            Assert.Equal(lang1.Lang, document.AlternateLanguages[0].Lang);
            Assert.Equal(lang1.Type, document.AlternateLanguages[0].Type);
            Assert.Equal(lang1.UID, document.AlternateLanguages[0].UID);

            var lang2 = new AlternateLanguage("WZ1iPyoAACkA7KtJ", "spanish", "article", "es-es");
            Assert.Equal(lang2.Id, document.AlternateLanguages[1].Id);
            Assert.Equal(lang2.Lang, document.AlternateLanguages[1].Lang);
            Assert.Equal(lang2.Type, document.AlternateLanguages[1].Type);
            Assert.Equal(lang2.UID, document.AlternateLanguages[1].UID);
        }

        [Fact]
        public async Task GetNumberTest()
        {
            Api api = await TestHelper.GetApi(TestHelper.Endpoint);
            var response = await api.Form("everything")
                .Ref(api.Master)
                .Query(DocumentId)
                .Submit();
            var doc = response.Results[0];
#pragma warning disable
            // startgist:1a6c8386fd678572d8b0:prismic-getNumber.cs
            // Number predicates
            var gt = Predicates.Gt("test_document.number", 10);
            var lt = Predicates.Lt("test_document.number", 20);
            var inRange = Predicates.InRange("test_document.number", 10, 20);

            // Accessing number fields
            decimal price = doc.GetNumber("test_document.number").Value;
            // endgist
            Assert.Equal(15m, price);
        }

        [Fact]
        public void GroupTest()
        {
            var resolver =
                prismic.DocumentLinkResolver.For(l => String.Format("http://localhost/{0}/{1}", l.Type, l.Id));

            var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"documents\":{\"type\":\"Group\",\"value\":[{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDejAEAAFwMyrW9\",\"type\":\"doc\",\"tags\":[],\"slug\":\"installing-meta-micro\"},\"isBroken\":false}},\"desc\":{\"type\":\"StructuredText\",\"value\":[{\"type\":\"paragraph\",\"text\":\"A detailed step by step point of view on how installing happens.\",\"spans\":[]}]}},{\"linktodoc\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UrDmKgEAALwMyrXA\",\"type\":\"doc\",\"tags\":[],\"slug\":\"using-meta-micro\"},\"isBroken\":false}}}]}}}}";
            var document = Document.Parse(JObject.Parse(json));
            // startgist:5926b0f6454f25e70350:prismic-group.cs
            var group = document.GetGroup("article.documents");
            foreach (GroupDoc doc in group.GroupDocs)
            {
                try
                {
                    fragments.StructuredText desc = doc.GetStructuredText("desc");
                    fragments.ILink link = doc.GetLink("linktodoc");
                }
                catch (Exception)
                {
                    // Missing key
                }
            }
            // endgist
            var firstDesc = group.GroupDocs[0].GetStructuredText("desc");
            Assert.Equal("<p>A detailed step by step point of view on how installing happens.</p>", firstDesc.AsHtml(resolver));
        }

        [Fact]
        public void LinkTest()
        {
            var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"source\":{\"type\":\"Link.document\",\"value\":{\"document\":{\"id\":\"UlfoxUnM0wkXYXbE\",\"type\":\"product\",\"tags\":[\"Macaron\"],\"slug\":\"dark-chocolate-macaron\"},\"isBroken\":false}}}}}";
            var document = Document.Parse(JObject.Parse(json));
            // startgist:ef7313f73b0a9488fb47:prismic-link.cs
            var resolver =
                prismic.DocumentLinkResolver.For(l => String.Format("http://localhost/{0}/{1}", l.Id, l.Slug));
            var source = document.GetLink("article.source");
            var url = source.GetUrl(resolver);
            // endgist
            Assert.Equal("http://localhost/UlfoxUnM0wkXYXbE/dark-chocolate-macaron", url);
        }

        [Fact]
        public void EmbedTest()
        {
            var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"video\":{\"type\":\"Embed\",\"value\":{\"oembed\":{\"provider_url\":\"http://www.youtube.com/\",\"type\":\"video\",\"thumbnail_height\":360,\"height\":270,\"thumbnail_url\":\"http://i1.ytimg.com/vi/baGfM6dBzs8/hqdefault.jpg\",\"width\":480,\"provider_name\":\"YouTube\",\"html\":\"<iframe width=\\\"480\\\" height=\\\"270\\\" src=\\\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\\\" frameborder=\\\"0\\\" allowfullscreen></iframe>\",\"author_name\":\"Siobhan Wilson\",\"version\":\"1.0\",\"author_url\":\"http://www.youtube.com/user/siobhanwilsonsongs\",\"thumbnail_width\":480,\"title\":\"Siobhan Wilson - All Dressed Up\",\"embed_url\":\"https://www.youtube.com/watch?v=baGfM6dBzs8\"}}}}}}";
            var document = Document.Parse(JObject.Parse(json));
            // startgist:dabf36e591c93029440a:prismic-embed.cs
            var video = document.GetEmbed("article.video");
            // Html is the code to include to embed the object, and depends on the embedded service
            var html = video.Html;
            // endgist
            Assert.Equal("<iframe width=\"480\" height=\"270\" src=\"http://www.youtube.com/embed/baGfM6dBzs8?feature=oembed\" frameborder=\"0\" allowfullscreen></iframe>", html);
        }

        [Fact]
        public void ColorTest()
        {
            var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"background\":{\"type\":\"Color\",\"value\":\"#000000\"}}}}";
            var document = Document.Parse(JObject.Parse(json));
            // startgist:87cc65a8d1d02f4b4342:prismic-color.cs
            var bgcolor = document.GetColor("article.background");
            var hex = bgcolor.Hex;
            // endgist
            Assert.Equal("#000000", hex);
        }

        [Fact]
        public void GeopointTest()
        {
            var json = "{\"id\":\"abcd\",\"type\":\"article\",\"href\":\"\",\"slugs\":[],\"tags\":[],\"data\":{\"article\":{\"location\":{\"type\":\"GeoPoint\",\"value\":{\"latitude\":48.877108,\"longitude\":2.333879}}}}}";
            var document = Document.Parse(JObject.Parse(json));
            fragments.GeoPoint place = document.GetGeoPoint("article.location");
#pragma warning disable IDE0059 
            var coordinates = place.Latitude + "," + place.Longitude;
            // endgist
#pragma warning restore IDE0059 // Variable is assigned but its value is never used
            Assert.Equal(48.877108, place.Latitude);
            Assert.Equal(2.333879, place.Longitude);
        }

        [Fact]
        public void PublishDateTest()
        {
            var document = Fixtures.GetDocument("language.json");
            Assert.Equal(new DateTime(2017, 1, 13, 11, 45, 21, DateTimeKind.Utc), document.FirstPublicationDate.Value.ToUniversalTime());
            Assert.Equal(new DateTime(2017, 2, 21, 16, 5, 19, DateTimeKind.Utc), document.LastPublicationDate.Value.ToUniversalTime());
        }
    }
}
