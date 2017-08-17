using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace prismic.tests
{
	[TestClass()]
	public class FragmentsTests
	{
		[TestMethod()]
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

		[TestMethod()]
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

		[TestMethod()]
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

		[TestMethod]
		public void ShouldParseTimestamp()
		{
			string text = System.IO.File.ReadAllText("fixtures/fragments.json");
			var json = JToken.Parse(text);
			var document = Document.Parse(json);
			var timestamp = document.GetTimestamp("article.date");
			Assert.AreEqual (2016, timestamp.Value.Year);
		}

		[TestMethod]
		public async Task ShouldAccessImage()
		{
			var url = "https://test-public.prismic.io/api";
			Api api = await prismic.Api.Get(url);
			var document = await api.GetByID("Uyr9sgEAAGVHNoFZ");
			var resolver = prismic.DocumentLinkResolver.For (l => String.Format ("http://localhost/{0}/{1}", l.Type, l.Id));
			var maybeImgView = document.GetImageView ("article.illustration", "icon");
			Assert.IsNotNull (maybeImgView);

			var html = maybeImgView.AsHtml(resolver);

			var someurl = "https://prismic-io.s3.amazonaws.com/test-public/9f5f4e8a5d95c7259108e9cfdde953b5e60dcbb6.jpg";
			var expect = String.Format (@"<img alt=""some alt text"" src=""{0}"" width=""100"" height=""100"" />", someurl);

			Assert.AreEqual(expect, html);
		}

		[TestMethod]
		public void ShouldParseOEmbed()
		{
			string text = System.IO.File.ReadAllText("fixtures/soundcloud.json");
			var json = JToken.Parse(text);
			var structuredText = prismic.fragments.StructuredText.Parse(json);
			prismic.fragments.StructuredText.Embed soundcloud = (prismic.fragments.StructuredText.Embed)structuredText.Blocks [0];
			prismic.fragments.StructuredText.Embed youtube = (prismic.fragments.StructuredText.Embed)structuredText.Blocks [1];

			Assert.IsNull (soundcloud.Obj.Width);
			Assert.AreEqual (youtube.Obj.Width, 480);
		}

	}
}
