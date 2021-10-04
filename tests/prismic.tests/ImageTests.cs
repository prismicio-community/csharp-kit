using prismic.fragments;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class ImageTests
    {
        private readonly Image _image;

        public ImageTests()
        {
            var document = Fixtures.GetDocument("image.json");

            _image = document.GetImage("test.image");
        }

        [Theory]
        [InlineData("main", true)]
        [InlineData("sm", true)]
        [InlineData("lg", false)]
        public void HasView_returns_expected_results(string viewName, bool expectedValue)
            => Assert.Equal(expectedValue, _image.HasView(viewName));

        [Theory]
        [InlineData("main", true)]
        [InlineData("sm", true)]
        [InlineData("lg", false)]
        public void TryGetView_returns_true_for_main(string viewName, bool expectedValue)
        {
            Assert.Equal(expectedValue, _image.TryGetView(viewName, out var view));
            if (expectedValue == true)
                Assert.NotNull(view);
            else
                Assert.Null(view);
        }
    }
}
