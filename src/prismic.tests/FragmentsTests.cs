using NUnit.Framework;
using prismic;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;

namespace prismic.tests
{
	[TestFixture ()]
	public class FragmentsTests
	{
		[Test ()]
		public async Task ShouldAccessGroupField()
		{
			var url = "https://micro.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var form = api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.type, ""docchapter"")]]");

			var response = await form.Submit();
			var document = response.Results.First();
			var group = document.GetGroup ("docchapter.docs");
			Assert.IsNotNull (group, "group was not found");

			var firstDoc = group.GroupDocs[0];
			Assert.IsNotNull (firstDoc, "doc was not found");

			var link = firstDoc.GetLink ("linktodoc");
			Assert.IsNotNull (link, "link was not found");
		}

		[Test ()]
		public async Task ShouldSerializeGroupToHTML()
		{
			var url = "https://micro.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.type, ""docchapter"")]]").Submit();

			var document = response.Results[1];
			var group = document.GetGroup ("docchapter.docs");

			Assert.IsNotNull (group, "group was not found");

			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));

			var html = group.AsHtml(resolver);
			Assert.IsNotNull (html);
			Assert.AreEqual(@"<section data-field=""linktodoc""><a href=""http://localhost/doc/UrDejAEAAFwMyrW9"">installing-meta-micro</a></section><section data-field=""desc""><p>Just testing another field in a group section.</p></section><section data-field=""linktodoc""><a href=""http://localhost/doc/UrDmKgEAALwMyrXA"">using-meta-micro</a></section>", html);
		}

		[Test ()]
		public async Task ShouldAccessMediaLink()
		{
			var url = "https://test-public.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.id, ""Uyr9_wEAAKYARDMV"")]]").Submit();

			var document = response.Results.First();
			var link = document.GetLink ("test-link.related");
			Assert.IsNotNull (link, "link was not found");
			Assert.AreEqual ("baastad.pdf", ((fragments.FileLink)link).Filename);

		}

		[Test ()]
		public async Task ShouldAccessFirstLinkInMultipleDocumentLink()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXba"")]]").Submit();
			var document = response.Results[0];
			var link = document.GetLink ("job-offer.location");
			Assert.IsNotNull (link, "link was not found");
			Assert.AreEqual ("paris-saint-lazare", ((fragments.DocumentLink)link).Slug);
		}

		[Test ()]
		public async Task ShouldSerializeHTMLWithCustomOutput()
		{
			var resolver = prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var serializer = prismic.HtmlSerializer.For ((elt, body) => {
				if (elt is fragments.StructuredText.Hyperlink) {
					var link = ((fragments.StructuredText.Hyperlink)elt).Link;
					if (link is fragments.DocumentLink) {
						var doclink = ((fragments.DocumentLink)link);
						return String.Format("<a class=\"some-link\" href=\"{0}\">{1}</a>", resolver.Resolve(doclink), body);
					}
				}
				if (elt is fragments.StructuredText.Image) {
					var imageview = ((fragments.StructuredText.Image)elt).View;
					return imageview.AsHtml(resolver);
				}
				return null;
			});

			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbf"")]]").Submit();

			var document = response.Results.First();
			var text = document.GetStructuredText ("article.content");
			var html = text.AsHtml(resolver, serializer);
			Assert.AreEqual("<h2>A tale of pastry and passion</h2>"
				+ "<p>As a child, Jean-Michel Pastranova learned the art of fine cuisine from his grand-father, Jacques Pastranova, who was the creator of the &quot;taste-design&quot; art current, and still today an unmissable reference of forward-thinking in cuisine. At first an assistant in his grand-father&#39;s kitchen, Jean-Michel soon found himself fascinated by sweet flavors and the tougher art of pastry, drawing his own path in the ever-changing cuisine world.</p>"
				+ "<p>In 1992, the first Les Bonnes Choses store opened on rue Saint-Lazare, in Paris (<a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbb\">we&#39;re still there!</a>), much to everyone&#39;s surprise; indeed, back then, it was very surprising for a highly promising young man with a preordained career as a restaurant chef, to open a pastry shop instead. But soon enough, contemporary chefs understood that Jean-Michel had the drive to redefine a new nobility to pastry, the same way many other kinds of cuisine were being qualified as &quot;fine&quot;.</p>"
				+ "<p>In 1996, meeting an overwhelming demand, Jean-Michel Pastranova opened <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbP\">a second shop on Paris&#39;s Champs-&#201;lys&#233;es</a>, and <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbr\">a third one in London</a>, the same week! Eventually, Les Bonnes Choses gained an international reputation as &quot;a perfection so familiar and new at the same time, that it will feel like a taste travel&quot; (New York Gazette), &quot;the finest balance between surprise and comfort, enveloped in sweetness&quot; (The Tokyo Tribune), &quot;a renewal of the pastry genre (...), the kind that changed the way pastry is approached globally&quot; (The San Francisco Gourmet News). Therefore, it was only a matter of time before Les Bonnes Choses opened shops in <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbc\">New York</a> (2000) and <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbU\">Tokyo</a> (2004).</p>"
				+ "<p>In 2013, Jean-Michel Pastranova stepped down as the CEO and Director of Workshops, remaining a senior advisor to the board and to the workshop artists; he passed the light on to Selena, his daugther, who initially learned the art of pastry from him. Passion for great food runs in the Pastranova family...</p>"
				+ "<img alt=\"\" src=\"https://prismic-io.s3.amazonaws.com/lesbonneschoses/df6c1d87258a5bfadf3479b163fd85c829a5c0b8.jpg\" width=\"800\" height=\"533\" />"
				+ "<h2>Our main value: our customers&#39; delight</h2>"
				+ "<p>Our every action is driven by the firm belief that there is art in pastry, and that this art is one of the dearest pleasures one can experience.</p>"
				+ "<p>At Les Bonnes Choses, people preparing your macarons are not simply &quot;pastry chefs&quot;: they are &quot;<a class=\"some-link\" href=\"http://localhost/job-offer/UlfoxUnM0wkXYXba\">ganache specialists</a>&quot;, &quot;<a class=\"some-link\" href=\"http://localhost/job-offer/UlfoxUnM0wkXYXbQ\">fruit experts</a>&quot;, or &quot;<a class=\"some-link\" href=\"http://localhost/job-offer/UlfoxUnM0wkXYXbn\">oven instrumentalists</a>&quot;. They are the best people out there to perform the tasks they perform to create your pastry, giving it the greatest value. And they just love to make their specialized pastry skill better and better until perfection.</p>"
				+ "<p>Of course, there is a workshop in each <em>Les Bonnes Choses</em> store, and every pastry you buy was made today, by the best pastry specialists in your country.</p>"
				+ "<p>However, the very difficult art of creating new concepts, juggling with tastes and creating brand new, powerful experiences, is performed every few months, during our &quot;<a class=\"some-link\" href=\"http://localhost/blog-post/UlfoxUnM0wkXYXbl\">Pastry Art Brainstorms</a>&quot;. During the event, the best pastry artists in the world (some working for <em>Les Bonnes Choses</em>, some not) gather in Paris, and showcase the experiments they&#39;ve been working on; then, the other present artists comment on the piece, and iterate on it together, in order to make it the best possible masterchief!</p>"
				+ "<p>The session is presided by Jean-Michel Pastranova, who then selects the most delightful experiences, to add it to <em>Les Bonnes Choses</em>&#39;s catalogue.</p>", html);
		}

		[Test ()]
		public async Task ShouldFindAllLinksInMultipleDocumentLink()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXba"")]]").Submit();

			var document = response.Results.First();
			var links = document.GetAll ("job-offer.location");
			Assert.AreEqual (3, links.Count());
			Assert.AreEqual ("paris-saint-lazare", ((fragments.DocumentLink)links[0]).Slug);
			Assert.AreEqual ("tokyo-roppongi-hills", ((fragments.DocumentLink)links[1]).Slug);
		}

		[Test ()]
		public async Task ShouldAccessStructuredText()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]").Submit();

			var document = response.Results.First();
			var maybeText = document.GetStructuredText ("blog-post.body");
			Assert.IsNotNull (maybeText);
		}

		[Test ()]
		public void ShouldAccessAndSerializeSlices()
		{
			var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var path = string.Format("{0}{1}fixtures{1}slices.json", directory, Path.DirectorySeparatorChar);
			string text = System.IO.File.ReadAllText(path);
			var json = JToken.Parse(text);
			var document = Document.Parse(json);
			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));

			var slices = document.GetSliceZone("article.blocks");
			Assert.AreEqual(slices.AsHtml(resolver),
				"<div data-slicetype=\"features\" class=\"slice\"><section data-field=\"illustration\"><img alt=\"\" src=\"https://wroomdev.s3.amazonaws.com/toto/db3775edb44f9818c54baa72bbfc8d3d6394b6ef_hsf_evilsquall.jpg\" width=\"4285\" height=\"709\" /></section>"
				+ "<section data-field=\"title\"><span class=\"text\">c&#39;est un bloc features</span></section></div>"
				+ "<div data-slicetype=\"text\" class=\"slice\"><p>C&#39;est un bloc content</p></div>");
		}

		[Test ()]
		public async Task ShouldQueryWithPredicate()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (Predicates.at("document.id", "UlfoxUnM0wkXYXbX")).Submit();

			var document = response.Results.First();
			var maybeText = document.GetStructuredText ("blog-post.body");
			Assert.IsNotNull (maybeText);
		}

		[Test ()]
		public async Task ShouldAccessImage()
		{
			var url = "https://test-public.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything").Ref(api.Master).Query (@"[[:d = at(document.id, ""Uyr9sgEAAGVHNoFZ"")]]").Submit();

			var resolver = prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var document = response.Results.First();
			var maybeImgView = document.GetImageView ("article.illustration", "icon");
			Assert.IsNotNull (maybeImgView);

			var html = maybeImgView.AsHtml(resolver);

			var someurl = "https://prismic-io.s3.amazonaws.com/test-public/9f5f4e8a5d95c7259108e9cfdde953b5e60dcbb6.jpg";
			var expect = String.Format (@"<img alt=""some alt text"" src=""{0}"" width=""100"" height=""100"" />", someurl);

			Assert.AreEqual(expect, html);
		}

		[Test ()]
		public async Task ShouldFetchLinksFragments()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var response = await api.Form("everything")
				.FetchLinks("blog-post.author")
				.Ref(api.Master)
				.Query (Predicates.at("document.id", "UlfoxUnM0wkXYXbt"))
				.Submit();

			var document = response.Results.First();

			var link = (prismic.fragments.DocumentLink)document.GetLink("blog-post.relatedpost[0]");
			Console.WriteLine ("Got the link: " + link.Fragments.Count);
			Assert.AreEqual(
				"John M. Martelle, Fine Pastry Magazine",
				link.GetText("blog-post.author")
			);
		}

		[Test ()]
		public void ShouldParseOEmbed()
		{
			var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var path = string.Format("{0}{1}fixtures{1}soundcloud.json", directory, Path.DirectorySeparatorChar);
			string text = System.IO.File.ReadAllText(path);
			var json = JToken.Parse(text);
			var structuredText = prismic.fragments.StructuredText.Parse(json);
			prismic.fragments.StructuredText.Embed soundcloud = (prismic.fragments.StructuredText.Embed)structuredText.Blocks [0];
			prismic.fragments.StructuredText.Embed youtube = (prismic.fragments.StructuredText.Embed)structuredText.Blocks [1];

			Assert.IsNull (soundcloud.Obj.Width);
			Assert.AreEqual (youtube.Obj.Width, 480);
		}

	}
}
