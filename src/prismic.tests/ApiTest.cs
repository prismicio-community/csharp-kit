using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace prismic.tests
{
	[TestClass]
	public class ApiTest
	{
		[TestMethod]
		public void GetPrivateApiWithoutAuthorizationTokenShouldThrow()
		{
            Assert.ThrowsExceptionAsync<AggregateException>(() => Api.Get("https://private-test.prismic.io/api"));
		}

		[TestMethod]
		public void GetPrivateApiWithInvalidTokenShouldThrow()
		{
			Assert.ThrowsExceptionAsync<AggregateException>(()=>  Api.Get ("https://private-test.prismic.io/api", "dummy token"));
		}
	}
}

