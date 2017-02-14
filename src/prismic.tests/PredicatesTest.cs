﻿

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace prismic.tests
{
	[TestClass]
	public class PredicatesTest
	{
		[TestMethod]
		public void testAtPredicate() {
			var p = Predicates.at("document.type", "blog-post");
			Assert.AreEqual("[:d = at(document.type, \"blog-post\")]", p.q());
		}

		[TestMethod]
		public void testAnyPredicate() {
			var p = Predicates.any("document.tags", new string[] { "Macaron", "Cupcakes" });
			Assert.AreEqual("[:d = any(document.tags, [\"Macaron\",\"Cupcakes\"])]", p.q());
		}

		[TestMethod]
		public void testNumberLT() {
			var p = Predicates.lt("my.product.price", 4.2);
			Assert.AreEqual("[:d = number.lt(my.product.price, 4.2)]", p.q());
		}

		[TestMethod]
		public void testNumberInRange() {
			var p = Predicates.inRange("my.product.price", 2, 4);
			Assert.AreEqual("[:d = number.inRange(my.product.price, 2, 4)]", p.q());
		}

		[TestMethod]
		public void testMonthAfter() {
			var p = Predicates.monthAfter("my.blog-post.publication-date", Predicates.Month.April);
			Assert.AreEqual("[:d = date.month-after(my.blog-post.publication-date, \"April\")]", p.q());
		}

		[TestMethod]
		public void testGeopointNear() {
			var p = Predicates.near("my.store.coordinates", 40.689757, -74.0451453, 15);
			Assert.AreEqual("[:d = geopoint.near(my.store.coordinates, 40.689757, -74.0451453, 15)]", p.q());
		}

	}


}

