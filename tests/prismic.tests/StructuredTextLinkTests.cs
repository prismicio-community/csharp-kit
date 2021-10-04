using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class StructuredTextLinkTests
    {
        [Fact]
        public void StructuredText_correctly_renders_links()
        {
            var document = Fixtures.GetDocument("structuredtext_links.json");
            var structuredText = document.GetStructuredText("test.text_block");

            var html = structuredText.AsHtml(DocumentLinkResolver.For(l => $"/test/{l.Uid}"));

            var expected = "<p>This is a structured text field with a <a href=\"/test/test-uid\">document link</a>, <a href=\"https://example.com\">web link</a> &amp; <a href=\"https://images.prismic.io/test.jpg?auto=compress,format\">media item</a>.</p>";

            Assert.Equal(expected, html);
        }
    }
}
