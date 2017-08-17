using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using prismic.fragments;

namespace prismic.tests
{
	[TestFixture()]
	public class SliceZoneParsingTests
	{
		public static JToken GetFixture(String file)
		{
			var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var path = string.Format("{0}{1}fixtures{1}{2}", directory, Path.DirectorySeparatorChar, file);
			string text = System.IO.File.ReadAllText(path);
			return JToken.Parse(text);
		}

		[Test ()]
		public void ShouldParseSimpleSlices()
		{
			var document = Document.Parse(GetFixture("simple_slices.json"));
			var resolver = prismic.DocumentLinkResolver.For(doc => String.Format ("http://localhost/{0}/{1}", doc.Type, doc.Id));
			var slices = document.GetSliceZone("article.blocks");

			Assert.AreEqual(slices.AsHtml(resolver),
				"<div data-slicetype=\"features\" class=\"slice\"><section data-field=\"illustration\"><img alt=\"\" src=\"https://wroomdev.s3.amazonaws.com/toto/db3775edb44f9818c54baa72bbfc8d3d6394b6ef_hsf_evilsquall.jpg\" width=\"4285\" height=\"709\" /></section>"
				+ "<section data-field=\"title\"><span class=\"text\">c&#39;est un bloc features</span></section></div>"
				+ "<div data-slicetype=\"text\" class=\"slice\"><p>C&#39;est un bloc content</p></div>");
		}

		[Test ()]
		public void ShouldGetFirstItemOfSimpleSlices()
		{
			var document = Document.Parse(GetFixture("simple_slices.json"));
			var sliceZone = document.GetSliceZone("article.blocks");
			SimpleSlice firstSlice = (prismic.fragments.SimpleSlice)sliceZone.Slices[0];
			Group group = (Group)firstSlice.Value;
			String expectedUrl = "https://wroomdev.s3.amazonaws.com/toto/db3775edb44f9818c54baa72bbfc8d3d6394b6ef_hsf_evilsquall.jpg";
			Assert.AreEqual(expectedUrl, group.GroupDocs[0].GetImage("illustration").GetView("main").Url);
		}

		[Test ()]
		public void ShouldParseCompositeSlices()
		{
			var document = Document.Parse(GetFixture("composite_slices.json"));
			var resolver = DocumentLinkResolver.For(doc => String.Format ("http://localhost/{0}/{1}", doc.Type, doc.Id));
			var sliceZone = document.GetSliceZone("page.page_content");
			Assert.AreEqual(sliceZone.AsHtml(resolver),
				"<div data-slicetype=\"text\" class=\"slice levi-label\"><section data-field=\"rich_text\"><p>Here is paragraph 1.</p><p>Here is paragraph 2.</p></section></div>"
				+ "<div data-slicetype=\"image_gallery\" class=\"slice\"><section data-field=\"gallery_title\"><h2>Image Gallery</h2></section>"
				+ "<section data-field=\"image\"><img alt=\"\" src=\"https://prismic-io.s3.amazonaws.com/levi-templeting%2Fdc0bfab3-d222-44a6-82b8-c74f8cdc6a6b_200_s.gif\" width=\"267\" height=\"200\" /></section>"
				+ "<section data-field=\"image\"><img alt=\"\" src=\"https://prismic-io.s3.amazonaws.com/levi-templeting/83c03dac4a604ac2e97e285e60034c641abd73b6_image2.jpg\" width=\"400\" height=\"369\" /></section></div>");
		}

		[Test ()]
		public void ShouldGetFirstItemOfCompositeSlices()
		{
			var document = Document.Parse(GetFixture("composite_slices.json"));
			var resolver = prismic.DocumentLinkResolver.For(doc => String.Format ("http://localhost/{0}/{1}", doc.Type, doc.Id));
			SliceZone sliceZone = document.GetSliceZone("page.page_content");
			CompositeSlice firstSlice = (CompositeSlice)sliceZone.Slices[0];
			StructuredText richText = firstSlice.GetPrimary().GetStructuredText("rich_text");
			Assert.AreEqual("<p>Here is paragraph 1.</p><p>Here is paragraph 2.</p>", richText.AsHtml(resolver));

			CompositeSlice secondSlice = (CompositeSlice)sliceZone.Slices[1];
			String expectedUrl = "https://prismic-io.s3.amazonaws.com/levi-templeting%2Fdc0bfab3-d222-44a6-82b8-c74f8cdc6a6b_200_s.gif";
			Assert.AreEqual(expectedUrl, secondSlice.GetItems().GroupDocs[0].GetImage("image").GetView("main").Url);
		}
	}
}
