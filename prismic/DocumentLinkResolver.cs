using System;

namespace prismic
{
	public abstract class DocumentLinkResolver
	{
	
		public abstract String Resolve(fragments.DocumentLink link);

		public String Resolve(Document doc) {
			return Resolve(doc.AsDocumentLink());
		}

		public String GetTitle(fragments.DocumentLink link) {
			return null;
		}

		public static DocumentLinkResolver For(System.Func<fragments.DocumentLink, string> resolver) {
			return new LambdaDocumentLinkResolver(resolver);
		}

	}

	public class LambdaDocumentLinkResolver: DocumentLinkResolver {
		private System.Func<fragments.DocumentLink, string> f;

		public override String Resolve(fragments.DocumentLink link) {
			return f(link);
		}

		public LambdaDocumentLinkResolver(System.Func<fragments.DocumentLink, string> f) {
			this.f = f;
		}

	}

}

