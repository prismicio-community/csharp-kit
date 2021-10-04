using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class StructuredTextSpanTests
    {
        [Fact]
        public void StructuredText_handles_emojis_with_adjacent_spans()
        {
            //Given
            var document = Fixtures.GetDocument("structuredtext_spans.json");
            var structuredText = document.GetStructuredText("test.text_block");

            //When
            var html = structuredText.AsHtml(DocumentLinkResolver.For(l => $"/"));

            //Then
            var expected = "<p>🤦‍♂️ 👏🏿 <span class=\"test\">This</span> is a structured text field with a label and an emoji.</p>";
            Assert.Equal(expected, html);
        }

        [Fact]
        public void StructuredText_handles_emojis_with_emoji_wrapped_in_label()
        {
            //Given
            var document = Fixtures.GetDocument("structuredtext_spans.json");
            var structuredText = document.GetStructuredText("test.wrapped_emoji");
            

            //When
            var html = structuredText.AsHtml(DocumentLinkResolver.For(l => $"/"));

            //Then
            var expected = "<p><span class=\"test\">👏🏿</span> A label wrapped emoji.</p>";
            Assert.Equal(expected, html);
        }
    }
}
