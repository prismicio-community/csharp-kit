using NUnit.Framework;
using prismic;
using System;
using System.Linq;
using System.ComponentModel;

namespace prismic.tests
{
	[TestFixture ()]
	public class FragmentsTests
	{
		[Test ()]
		public void ShouldAccessGroupField()
		{
			var url = "https://micro.prismic.io/api";
			Api api = prismic.Api.Get(url);
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.type, ""docchapter"")]]").SubmitableAsTask();

			var document = form.Submit().Result.results.First();
			var maybeGroup = document.GetGroup ("docchapter.docs");
			Assert.IsTrue (maybeGroup.Exists(), "group was not found");

			var maybeFirstDoc = maybeGroup.Value.Item.FirstOrDefault ();
			Assert.IsNotNull (maybeFirstDoc, "doc was not found");

			var maybeLink = maybeFirstDoc.GetLink ("linktodoc");
			Assert.IsTrue (maybeLink.Exists(), "link was not found");
		}

		[Test ()]
		public void ShouldSerializeGroupToHTML()
		{
			var url = "https://micro.prismic.io/api";
			Api api = prismic.Api.Get(url);
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.type, ""docchapter"")]]").SubmitableAsTask();

			var document = form.Submit().Result.results.ElementAt(1);
			var maybeGroup = document.GetGroup ("docchapter.docs");

			Assert.IsTrue (maybeGroup.Exists(), "group was not found");

			var resolver =
				prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));

			var html = maybeGroup.BindAsHtml(resolver);
			Assert.IsTrue (html.Exists ());
			Console.WriteLine (html.Value);
			Assert.AreEqual(@"<section data-field=""linktodoc""><a href=""http://localhost/doc/UrDejAEAAFwMyrW9"">installing-meta-micro</a></section>
<section data-field=""desc""><p>Just testing another field in a group section.</p></section>
<section data-field=""linktodoc""><a href=""http://localhost/doc/UrDmKgEAALwMyrXA"">using-meta-micro</a></section>", html.Value);
		}

		[Test ()]
		public void ShouldAccessMediaLink()
		{
			var url = "https://test-public.prismic.io/api";
			Api api = (prismic.Api.Get(url, new prismic.ApiInfra.NoCache<prismic.Api.Response>(), (l, m) => {})).Result;
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.id, ""Uyr9_wEAAKYARDMV"")]]");

			var document = form.Submit().Results.First();
			var maybeLink = document.GetLink ("test-link.related");
			Assert.IsTrue (maybeLink.Exists(), "link was not found");
			Assert.AreEqual ("baastad.pdf", maybeLink.BindAsMediaLink ().Value.filename);

		}

		[Test ()]
		public void ShouldAccessFirstLinkInMultipleDocumentLink()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = (prismic.Api.Get(url, new prismic.ApiInfra.NoCache<prismic.Api.Response>(), (l, m) => {}));
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXba"")]]");

			var document = form.Submit().Results.First();
			var maybeLink = document.GetLink ("job-offer.location");
			Assert.IsTrue (maybeLink.Exists(), "link was not found");
			Assert.AreEqual ("paris-saint-lazare", maybeLink.BindAsDocumentLink ().Value.slug);
		}

		[Test ()]
		public void ShouldSerializeHTMLWithCustomOutput()
		{
			var resolver = prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.typ, l.id));
			var serializer = prismic.HtmlSerializer.For ((elt, body) => {
				if (elt is Fragments.Span.Hyperlink) {
					var link = ((Fragments.Span.Hyperlink)elt).Item.Item3;
					if (link is Fragments.Link.DocumentLink) {
						var doclink = ((Fragments.Link.DocumentLink)link).Item;
						return String.Format("<a class=\"some-link\" href=\"{0}\">{1}</a>", resolver.Apply(doclink), body);
					}
				}
				if (elt is Fragments.Block.Image) {
					var imageview = ((Fragments.Block.Image)elt).Item;
					return imageview.AsHtml(resolver);
				}
				return null;
			});

			var url = "https://lesbonneschoses.prismic.io/api";
			Api api = (prismic.Api.Get(url, new prismic.ApiInfra.NoCache<prismic.Api.Response>(), (l, m) => {})).Result;
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbf"")]]");

			var document = form.Submit().Results.First();
			var text = document.GetStructuredText ("article.content").Value;
			var html = text.AsHtml(resolver, serializer);
			Assert.AreEqual("<h2>A tale of pastry and passion</h2>\n"
				+ "<p>As a child, Jean-Michel Pastranova learned the art of fine cuisine from his grand-father, Jacques Pastranova, who was the creator of the &quot;taste-design&quot; art current, and still today an unmissable reference of forward-thinking in cuisine. At first an assistant in his grand-father&#39;s kitchen, Jean-Michel soon found himself fascinated by sweet flavors and the tougher art of pastry, drawing his own path in the ever-changing cuisine world.</p>\n"
				+ "<p>In 1992, the first Les Bonnes Choses store opened on rue Saint-Lazare, in Paris (<a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbb\">we&#39;re still there!</a>), much to everyone&#39;s surprise; indeed, back then, it was very surprising for a highly promising young man with a preordained career as a restaurant chef, to open a pastry shop instead. But soon enough, contemporary chefs understood that Jean-Michel had the drive to redefine a new nobility to pastry, the same way many other kinds of cuisine were being qualified as &quot;fine&quot;.</p>\n"
				+ "<p>In 1996, meeting an overwhelming demand, Jean-Michel Pastranova opened <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbP\">a second shop on Paris&#39;s Champs-&#201;lys&#233;es</a>, and <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbr\">a third one in London</a>, the same week! Eventually, Les Bonnes Choses gained an international reputation as &quot;a perfection so familiar and new at the same time, that it will feel like a taste travel&quot; (New York Gazette), &quot;the finest balance between surprise and comfort, enveloped in sweetness&quot; (The Tokyo Tribune), &quot;a renewal of the pastry genre (...), the kind that changed the way pastry is approached globally&quot; (The San Francisco Gourmet News). Therefore, it was only a matter of time before Les Bonnes Choses opened shops in <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbc\">New York</a> (2000) and <a class=\"some-link\" href=\"http://localhost/store/UlfoxUnM0wkXYXbU\">Tokyo</a> (2004).</p>\n"
				+ "<p>In 2013, Jean-Michel Pastranova stepped down as the CEO and Director of Workshops, remaining a senior advisor to the board and to the workshop artists; he passed the light on to Selena, his daugther, who initially learned the art of pastry from him. Passion for great food runs in the Pastranova family...</p>\n"
				+ "<img alt=\"\" src=\"https://prismic-io.s3.amazonaws.com/lesbonneschoses/df6c1d87258a5bfadf3479b163fd85c829a5c0b8.jpg\" width=\"800\" height=\"533\" />\n"
				+ "<h2>Our main value: our customers&#39; delight</h2>\n"
				+ "<p>Our every action is driven by the firm belief that there is art in pastry, and that this art is one of the dearest pleasures one can experience.</p>\n"
				+ "<p>At Les Bonnes Choses, people preparing your macarons are not simply &quot;pastry chefs&quot;: they are &quot;<a class=\"some-link\" href=\"http://localhost/job-offer/UlfoxUnM0wkXYXba\">ganache specialists</a>&quot;, &quot;<a class=\"some-link\" href=\"http://localhost/job-offer/UlfoxUnM0wkXYXbQ\">fruit experts</a>&quot;, or &quot;<a class=\"some-link\" href=\"http://localhost/job-offer/UlfoxUnM0wkXYXbn\">oven instrumentalists</a>&quot;. They are the best people out there to perform the tasks they perform to create your pastry, giving it the greatest value. And they just love to make their specialized pastry skill better and better until perfection.</p>\n"
				+ "<p>Of course, there is a workshop in each <em>Les Bonnes Choses</em> store, and every pastry you buy was made today, by the best pastry specialists in your country.</p>\n"
				+ "<p>However, the very difficult art of creating new concepts, juggling with tastes and creating brand new, powerful experiences, is performed every few months, during our &quot;<a class=\"some-link\" href=\"http://localhost/blog-post/UlfoxUnM0wkXYXbl\">Pastry Art Brainstorms</a>&quot;. During the event, the best pastry artists in the world (some working for <em>Les Bonnes Choses</em>, some not) gather in Paris, and showcase the experiments they&#39;ve been working on; then, the other present artists comment on the piece, and iterate on it together, in order to make it the best possible masterchief!</p>\n"
				+ "<p>The session is presided by Jean-Michel Pastranova, who then selects the most delightful experiences, to add it to <em>Les Bonnes Choses</em>&#39;s catalogue.</p>", html);
		}

		[Test ()]
		public void ShouldFindAllLinksInMultipleDocumentLink()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api.Api api = (prismic.Api.Get(url, new prismic.ApiInfra.NoCache<prismic.Api.Response>(), (l, m) => {}));
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXba"")]]");

			var document = form.Submit().Result.results.First();
			var links = document.GetAll ("job-offer.location");
			Assert.AreEqual (3, links.Count());
			Assert.AreEqual ("paris-saint-lazare", FSharpOption<Fragments.Fragment>.Some(links.ElementAt(0)).BindAsDocumentLink ().Value.slug);
			Assert.AreEqual ("tokyo-roppongi-hills", FSharpOption<Fragments.Fragment>.Some(links.ElementAt(1)).BindAsDocumentLink ().Value.slug);
		}

		[Test ()]
		public void ShouldAccessStructuredText()
		{
			var url = "https://lesbonneschoses.prismic.io/api";
			Api.Api api = (prismic.extensions.Api.Get(url, new prismic.ApiInfra.NoCache<prismic.Api.Response>(), (l, m) => {})).Result;
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.id, ""UlfoxUnM0wkXYXbX"")]]").SubmitableAsTask();

			var document = form.Submit().Result.results.First();
			var maybeText = document.GetStructuredText ("blog-post.body");
			Assert.IsTrue (maybeText.Exists ());
		}


		[Test ()]
		public void ShouldAccessImage()
		{
			var url = "https://test-public.prismic.io/api";
			Api.Api api = (prismic.extensions.Api.Get(url, new prismic.ApiInfra.NoCache<prismic.Api.Response>(), (l, m) => {})).Result;
			var form = api.Forms["everything"].Ref(api.Master).Query (@"[[:d = at(document.id, ""Uyr9sgEAAGVHNoFZ"")]]").SubmitableAsTask();

			var resolver = prismic.extensions.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.typ, l.id));
			var document = form.Submit().Result.results.First();
			var maybeImgView = document.GetImageView ("article.illustration", "icon");
			Assert.IsTrue (maybeImgView.Exists ());

			var html = maybeImgView.BindAsHtml(resolver).Value;

			var someurl = "https://prismic-io.s3.amazonaws.com/test-public/9f5f4e8a5d95c7259108e9cfdde953b5e60dcbb6.jpg";
			var expect = String.Format (@"<img alt=""some alt text"" src=""{0}"" width=""100"" height=""100"" />", someurl);

			Assert.AreEqual(expect, html);
		}

	}
}
