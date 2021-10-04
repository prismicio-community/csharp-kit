using Xunit;

namespace prismic.AspNetCore.Tests
{
	public class PredicatesTest
	{
		[Fact]
		public void TestAtPredicate() {
			var p = Predicates.At("document.type", "blog-post");
			Assert.Equal("[:d = at(document.type, \"blog-post\")]", p.Q());
		}

		[Fact]
		public void TestAnyPredicate() {
			var p = Predicates.Any("document.tags", new string[] { "Macaron", "Cupcakes" });
			Assert.Equal("[:d = any(document.tags, [\"Macaron\",\"Cupcakes\"])]", p.Q());
		}

		[Fact]
		public void TestNumberLT() {
			var p = Predicates.Lt("my.product.price", 4.2);
			Assert.Equal("[:d = number.lt(my.product.price, 4.2)]", p.Q());
		}

		[Fact]
		public void TestNumberInRange() {
			var p = Predicates.InRange("my.product.price", 2, 4);
			Assert.Equal("[:d = number.inRange(my.product.price, 2, 4)]", p.Q());
		}

		[Fact]
		public void TestMonthAfter() {
			var p = Predicates.MonthAfter("my.blog-post.publication-date", Predicates.Months.April);
			Assert.Equal("[:d = date.month-after(my.blog-post.publication-date, \"April\")]", p.Q());
		}

		[Fact]
		public void TestGeopointNear() {
			var p = Predicates.Near("my.store.coordinates", 40.689757, -74.0451453, 15);
			Assert.Equal("[:d = geopoint.near(my.store.coordinates, 40.689757, -74.0451453, 15)]", p.Q());
		}

		[Fact]
		public void TestNot() {
			var p = Predicates.Not("my.store.coordinates", "value");
			Assert.Equal("[:d = not(my.store.coordinates, \"value\")]", p.Q());
		}

		[Fact]
		public void TestHas() {
			var p = Predicates.Has("my.store.coordinates");
			Assert.Equal("[:d = has(my.store.coordinates)]", p.Q());
		}

		[Fact]
		public void TestSimilar() {
			var p = Predicates.Similar("VkRmhykAAFA6PoBj", 10);
			Assert.Equal("[:d = similar(\"VkRmhykAAFA6PoBj\", 10)]", p.Q());
		}
	}
}

