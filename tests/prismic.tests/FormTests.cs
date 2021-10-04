using Xunit;
using prismic;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace prismic.AspNetCore.Tests
{
    public class FormTests
    {
        private readonly ApiData _apiData;

        public FormTests()
        {
            var rawApiData = Fixtures.Get("api_data.json");
            _apiData = ApiData.Parse(rawApiData);
        }

        [Fact]
        public void Set_intval_sets_value()
        {
            var form = GetSearchForm();
            form.Set("pageSize", 1);

            Assert.Contains("pageSize=1", form.ToString());
        }

        [Fact]
        public void Set_intval_throws_argument_exception_for_unknown_field()
        {
            var form = GetSearchForm();

            var ex = Assert.Throws<ArgumentException>(() => form.Set("unknown", 1));
            Assert.StartsWith("Unknown field", ex.Message);
        }

        [Fact]
        public void Set_intval_throws_argument_exception_non_integer_field()
        {
            var form = GetSearchForm();

            var ex = Assert.Throws<ArgumentException>(() => form.Set("q", 1));
            Assert.StartsWith("Cannot set an Integer value to field", ex.Message);
        }

        [Fact]
        public void Set_stringval_throws_argument_exception_non_integer_field()
        {
            var form = GetSearchForm();

            var ex = Assert.Throws<ArgumentException>(() => form.Set("unknown", "value"));
            Assert.StartsWith("Unknown field", ex.Message);
        }


        private Form.SearchForm GetSearchForm()
        {
            var cache = new Mock<ICache>();
            var logger = new Mock<ILogger<PrismicHttpClient>>();
            var client = TestHelper.CreatePrismicHttpClient(cache.Object, logger.Object);
            return new Form.SearchForm(client, _apiData.Forms["everything"]);
        }
    }
}
