using System;

namespace prismic
{
	public abstract class HtmlSerializer
	{
		public abstract String Serialize(fragments.StructuredText.Element element, String content);

		public static HtmlSerializer For(System.Func<Object, string, string> f) {
			return new LambdaHtmlSerializer(f);
		}
	}

	public class LambdaHtmlSerializer: HtmlSerializer {
		private System.Func<Object, string, string> f;

		public LambdaHtmlSerializer(System.Func<Object, string, string> f) {
			this.f = f;
		}

		public override string Serialize (fragments.StructuredText.Element element, String content)
		{
			return f(element, content);
		}

	}

}

