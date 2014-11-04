using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Threading.Tasks;
using prismic;

namespace prismic.tests
{
	[TestFixture ()]
	public class ApiTest
	{
		[Test ()]
		[ExpectedException(typeof(prismic.Error))]
		public void GetPrivateApiWithoutAuthorizationTokenShouldThrow()
		{
			Api.Get ("https://private-test.prismic.io/api");
		}

		[Test ()]
		[ExpectedException(typeof(prismic.Error))]
		public void GetPrivateApiWithInvalidTokenShouldThrow()
		{
			Api.Get ("https://private-test.prismic.io/api", "dummy token");
		}

		private void ExpectInnerException<ExT>(Action action, Func<ExT, bool> exceptionPredicate) where ExT : Exception
		{
			try {
				ThrowInner(action);
				Assert.Fail("expected exception was not raised");
			} catch (ExT ex) {
				exceptionPredicate (ex);
			} catch (Exception ex) {
				Assert.Fail(String.Format("unexpected type of exception happened: {0} {1}", ex.GetType().Name, ex.Message));
			}
		}

		private void ThrowInner(Action action) 
		{
			try {
				action();
			} catch (AggregateException ex) {
				throw ex.InnerException;
			}
		}

	}
}

