using System;

using System.Collections.Generic;

namespace prismic
{
	namespace fragments {
		public class StructuredText: Fragment {

			public interface Element {}

			public abstract class Block: Element {
				private String label;
				public String Label {
					get {
						return label;
					}
				}
				public Block(String label) {
					this.label = label;
				}
			}

			public abstract class TextBlock: Block {
				private String text;
				String Text {
					get {
						return text;
					}
				}
				private IList<Span> spans;
				IList<Span> Spans {
					get {
						return spans;
					}
				}
				public TextBlock(String text, IList<Span> spans, String label): base(label) {
					this.text = text;
					this.spans = spans;
				}
			}

			public class Heading: TextBlock {
				private int level;
				public int Level {
					get {
						return level;
					}
				}

				public Heading(String text, List<Span> spans, int level, String label): base(text, spans, label) {
					this.level = level;
				}

			}

			public class Paragraph: TextBlock {
				public Paragraph(String text, List<Span> spans, String label): base(text, spans, label) {}
			}

			public class Preformatted: TextBlock {
				public Preformatted(String text, List<Span> spans, String label): base(text, spans, label) {}
			}

			public class ListItem: TextBlock {
				private Boolean ordered;
				public Boolean IsOrdered {
					get {
						return ordered;
					}
				}

				public ListItem(String text, IList<Span> spans, Boolean ordered, String label): base(text, spans, label) {
					this.ordered = ordered;
				}

			}

			public class Image: Block {
				private fragments.Image.View view;
				public fragments.Image.View View {
					get {
						return view;
					}
				}

				public Image(fragments.Image.View view, String label): base(label) {
					this.view = view;
				}

				public String Url {
					get { return view.Url; }
				}

				public int Width {
					get { return view.Width; }
				}

				public int Height {
					get { return view.Height; }
				}

			}

			public class Embed: Block {
				private fragments.Embed obj;
				public fragments.Embed Obj {
					get { return obj; }
				}

				public Embed(fragments.Embed obj, String label): base(label) {
					this.obj = obj;
				}

			}

			public abstract class Span: Element {
				private int start;
				public int Start {
					get {
						return start;
					}
				}
				private int end;
				public int End {
					get {
						return end;
					}
				}
				public Span(int start, int end) {
					this.start = start;
					this.end = end;
				}
			}

			public class Em: Span {
				public Em(int start, int end): base(start, end) {}
			}

			public class Strong: Span {
				public Strong(int start, int end): base(start, end) {}
			}

			public class Hyperlink: Span {
				private Link link;
				public Link Link {
					get {
						return link;
					}
				}

				public Hyperlink(int start, int end, Link link): base(start, end) {
					this.link = link;
				}
			}

			public class LabelSpan: Span {
				private String label;
				public String Label {
					get {
						return label;
					}
				}

				public LabelSpan(int start, int end, String label): base(start, end) {
					this.label = label;
				}
			}
		}

	}

}

