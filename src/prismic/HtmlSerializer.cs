using System;

namespace prismic
{
	public abstract class HtmlSerializer
	{
		public abstract string Serialize(fragments.StructuredText.IElement element, string content);

        public static HtmlSerializer For(Func<object, string, string> f) 
			=> new LambdaHtmlSerializer(f);
    }

	public class LambdaHtmlSerializer: HtmlSerializer {
		private readonly Func<object, string, string> _f;

		public LambdaHtmlSerializer(Func<object, string, string> f) {
			_f = f;
		}

        public override string Serialize(fragments.StructuredText.IElement element, string content)
			=> _f(element, content);
    }

}

