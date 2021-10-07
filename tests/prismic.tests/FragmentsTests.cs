using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class FragmentTests
    {
        [Fact]
        public async Task ShouldAccessGroupField()
        {
            var url = "https://micro.prismic.io/api";
            Api api = await TestHelper.GetApi(url);
            var form = api.Form("everything").Ref(api.Master).Query(@"[[:d = at(document.type, ""docchapter"")]]");

            var response = await form.Submit();
            var document = response.Results.First();
            var group = document.GetGroup("docchapter.docs");
            Assert.NotNull(group);

            var firstDoc = group.GroupDocs[0];
            Assert.NotNull(firstDoc);

            var link = firstDoc.GetLink("linktodoc");
            Assert.NotNull(link);
        }

        [Fact]
        public async Task ShouldSerializeGroupToHTML()
        {
            var url = "https://micro.prismic.io/api";
            Api api = await TestHelper.GetApi(url);
            var response = await api.Form("everything").Ref(api.Master).Query(@"[[:d = at(document.type, ""docchapter"")]]").Submit();

            var document = response.Results[1];
            var group = document.GetGroup("docchapter.docs");

            Assert.NotNull(group);

            var resolver =
                prismic.DocumentLinkResolver.For(l => String.Format("http://localhost/{0}/{1}", l.Type, l.Id));

            var html = group.AsHtml(resolver);
            Assert.NotNull(html);
            Assert.Equal(@"<section data-field=""linktodoc""><a href=""http://localhost/doc/UrDejAEAAFwMyrW9"">installing-meta-micro</a></section><section data-field=""desc""><p>Just testing another field in a group section.</p></section><section data-field=""linktodoc""><a href=""http://localhost/doc/UrDmKgEAALwMyrXA"">using-meta-micro</a></section>", html);
        }

        [Fact]
        public async Task ShouldAccessMediaLink()
        {
            var url = "https://test-public.prismic.io/api";
			
            Api api = await TestHelper.GetApi(url);
            var response = await api.Form("everything").Ref(api.Master).Query(@"[[:d = at(document.id, ""Uyr9_wEAAKYARDMV"")]]").Submit();

            var document = response.Results.First();
            var link = document.GetLink("test-link.related");
            Assert.NotNull(link);
            Assert.Equal("baastad.pdf", ((fragments.FileLink)link).Filename);

        }

        [Fact]
        public void ShouldParseTimestamp()
        {
            var document = Fixtures.GetDocument("fragments.json");
            var timestamp = document.GetTimestamp("article.date");
            Assert.Equal(2016, timestamp.Value.Year);
        }

        [Fact]
        public void ShouldParseBoolean()
        {
            var document = Fixtures.GetDocument("fragments.json");
            var boolFragment = document.GetBoolean("article.bool");
            Assert.True(boolFragment.Value);
        }

        [Fact]
        public async Task ShouldAccessImage()
        {
            var url = "https://test-public.prismic.io/api";
			var api = await TestHelper.GetApi(url);
            var document = await api.GetByID("Uyr9sgEAAGVHNoFZ");
            var resolver = prismic.DocumentLinkResolver.For(l => String.Format("http://localhost/{0}/{1}", l.Type, l.Id));
            var maybeImgView = document.GetImageView("article.illustration", "icon");
            Assert.NotNull(maybeImgView);

            var html = maybeImgView.AsHtml(resolver);

            var someurl = "https://images.prismic.io/test-public/9f5f4e8a5d95c7259108e9cfdde953b5e60dcbb6.jpg?auto=compress,format";
            var expect = String.Format(@"<img alt=""some alt text"" src=""{0}"" width=""100"" height=""100"" />", someurl);

            Assert.Equal(expect, html);
        }

        [Fact]
        public async Task ShouldHaveImageView()
        {
            var url = "https://test-public.prismic.io/api";
			var api = await TestHelper.GetApi(url);
            var document = await api.GetByID("Uyr9sgEAAGVHNoFZ");
            var maybeImg = document.GetImage("article.illustration");
            
            Assert.NotNull(maybeImg);
            Assert.True(maybeImg.HasView("icon"));
        }

        [Fact]
        public async Task ShouldTryGetmageView()
        {
            var url = "https://test-public.prismic.io/api";
			var api = await TestHelper.GetApi(url);
            var document = await api.GetByID("Uyr9sgEAAGVHNoFZ");
            var maybeImg = document.GetImage("article.illustration");
            
            Assert.NotNull(maybeImg);
            var tryResult = maybeImg.TryGetView("icon", out var view);
            Assert.True(tryResult);
            Assert.NotNull(view);
        }

        [Fact]
        public void ShouldParseOEmbed()
        {
            var json = Fixtures.Get("soundcloud.json");
            var structuredText = prismic.fragments.StructuredText.Parse(json);
            prismic.fragments.StructuredText.Embed soundcloud = (prismic.fragments.StructuredText.Embed)structuredText.Blocks[0];
            prismic.fragments.StructuredText.Embed youtube = (prismic.fragments.StructuredText.Embed)structuredText.Blocks[1];

            Assert.Null(soundcloud.Obj.Width);
            Assert.Equal(480, youtube.Obj.Width);
        }

		[Fact]
		public void ShouldAccessRaw()
		{
		    var document = Fixtures.GetDocument("rawexample.json");
		    var authorRaw = document.GetRaw("test_type.author");
		    var authorsGroup = document.GetGroup("test_type.authors");
		    var authorRaw2 = authorsGroup.GroupDocs.FirstOrDefault().GetRaw("author_ref");
		    Assert.Equal(15, authorRaw.Value.Children().Count());
		    Assert.Equal(15, authorRaw2.Value.Children().Count());
        }

        [Fact]
        public void ShouldParseDate()
        {
            var date = fragments.Date.Parse(JToken.Parse("\"2020-02-20\""));
            
            Assert.NotNull(date);
            Assert.Equal(new DateTime(2020, 02, 20), date.Value);
        }

        [Fact]
        public void Returns_null_for_invalid_date()
        {
            var date = fragments.Date.Parse(JToken.Parse("\"bad date\""));
            
            Assert.Null(date);
        }
    }
}
