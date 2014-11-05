using System;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Web;

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
				public String Text {
					get {
						return text;
					}
				}
				private IList<Span> spans;
				public IList<Span> Spans {
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

				public Heading(String text, IList<Span> spans, int level, String label): base(text, spans, label) {
					this.level = level;
				}

			}

			public class Paragraph: TextBlock {
				public Paragraph(String text, IList<Span> spans, String label): base(text, spans, label) {}
			}

			public class Preformatted: TextBlock {
				public Preformatted(String text, IList<Span> spans, String label): base(text, spans, label) {}
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

			private IList<Block> blocks;
			public IList<Block> Blocks {
				get { return blocks; }
			}

			public StructuredText(IList<Block> blocks) {
				this.blocks = blocks;
			}

			public IList<Block> getBlocks() {
				return blocks;
			}

			public Heading getTitle() {
				foreach(Block block in blocks) {
					if(block is Heading) return (Heading)block;
				}
				return null;
			}

			public Paragraph getFirstParagraph() {
				foreach(Block block in blocks) {
					if(block is Paragraph) return (Paragraph)block;
				}
				return null;
			}

			public Preformatted getFirstPreformatted() {
				foreach(Block block in blocks) {
					if(block is Preformatted) return (Preformatted)block;
				}
				return null;
			}

			public Image getFirstImage() {
				foreach(Block block in blocks) {
					if(block is Image) return (Image)block;
				}
				return null;
			}

			private class BlockGroup {
				public String tag;
				public IList<Block> blocks;

				public BlockGroup(String tag, IList<Block> blocks) {
					this.tag = tag;
					this.blocks = blocks;
				}
			}

			public String AsHtml(IList<Block> blocks, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
				IList<BlockGroup> blockGroups = new List<BlockGroup>();
				foreach(Block block in blocks) {
					BlockGroup lastOne = blockGroups.Count == 0 ? null : blockGroups[blockGroups.Count - 1];
					if(lastOne != null && "ul" == lastOne.tag && block is ListItem && !((ListItem)block).IsOrdered) {
						lastOne.blocks.Add(block);
					}
					else if(lastOne != null && "ol" == lastOne.tag && block is ListItem && ((ListItem)block).IsOrdered) {
						lastOne.blocks.Add(block);
					}
					else if(block is ListItem && !((ListItem)block).IsOrdered) {
						BlockGroup newBlockGroup = new BlockGroup("ul", new List<Block>());
						newBlockGroup.blocks.Add(block);
						blockGroups.Add(newBlockGroup);
					}
					else if(block is ListItem && ((ListItem)block).IsOrdered) {
						BlockGroup newBlockGroup = new BlockGroup("ol", new List<Block>());
						newBlockGroup.blocks.Add(block);
						blockGroups.Add(newBlockGroup);
					}
					else {
						BlockGroup newBlockGroup = new BlockGroup(null, new List<Block>());
						newBlockGroup.blocks.Add(block);
						blockGroups.Add(newBlockGroup);
					}
				}
				var html = "";
				foreach(BlockGroup blockGroup in blockGroups) {
					if(blockGroup.tag != null) {
						html += ("<" + blockGroup.tag + ">");
						foreach(Block block in blockGroup.blocks) {
							html += (asHtml(block, linkResolver, htmlSerializer));
						}
						html += ("</" + blockGroup.tag + ">");
					} else {
						foreach(Block block in blockGroup.blocks) {
							html += (asHtml(block, linkResolver, htmlSerializer));
						}
					}
				}
				return html;
			}

			public String asHtml(Block block, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
				String content = "";
				if(block is StructuredText.Heading) {
					StructuredText.Heading heading = (StructuredText.Heading)block;
					content = insertSpans(heading.Text, heading.Spans, linkResolver, htmlSerializer);
				}
				else if(block is StructuredText.Paragraph) {
					StructuredText.Paragraph paragraph = (StructuredText.Paragraph)block;
					content = insertSpans(paragraph.Text, paragraph.Spans, linkResolver, htmlSerializer);
				}
				else if(block is StructuredText.Preformatted) {
					StructuredText.Preformatted paragraph = (StructuredText.Preformatted)block;
					content = insertSpans(paragraph.Text, paragraph.Spans, linkResolver, htmlSerializer);
				}
				else if(block is StructuredText.ListItem) {
					StructuredText.ListItem listItem = (StructuredText.ListItem)block;
					content = insertSpans(listItem.Text, listItem.Spans, linkResolver, htmlSerializer);
				}

				if (htmlSerializer != null) {
					String customHtml = htmlSerializer.Serialize(block, content);
					if (customHtml != null) {
						return customHtml;
					}
				}
				String classCode = block.Label == null ? "" : (" class=\"" + block.Label + "\"");
				if(block is StructuredText.Heading) {
					StructuredText.Heading heading = (StructuredText.Heading)block;
					return ("<h" + heading.Level + classCode + ">" + content + "</h" + heading.Level + ">");
				}
				else if(block is StructuredText.Paragraph) {
					return ("<p" + classCode + ">" + content + "</p>");
				}
				else if(block is StructuredText.Preformatted) {
					return ("<pre" + classCode + ">" + content + "</pre>");
				}
				else if(block is StructuredText.ListItem) {
					return ("<li" + classCode + ">" + content + "</li>");
				}
				else if(block is StructuredText.Image) {
					StructuredText.Image image = (StructuredText.Image)block;
					String labelCode = block.Label == null ? "" : (" " + block.Label);
					return ("<p class=\"block-img" + labelCode + "\">" + image.View.AsHtml(linkResolver) + "</p>");
				}
				else if(block is StructuredText.Embed) {
					StructuredText.Embed embed = (StructuredText.Embed)block;
					return (embed.Obj.AsHtml());
				}
				return "";
			}

			private static String serialize(Span span, String content, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
				if (htmlSerializer != null) {
					String customHtml = htmlSerializer.Serialize(span, content);
					if (customHtml != null) {
						return customHtml;
					}
				}
				if (span is Strong) {
					return "<strong>" + content + "</strong>";
				}
				if (span is Em) {
					return "<em>" + content + "</em>";
				}
				if (span is LabelSpan) {
					return "<span class=\"" + ((LabelSpan)span).Label + "\">" + content + "</span>";
				}
				if (span is Hyperlink) {
					Hyperlink hyperlink = (Hyperlink)span;
					if(hyperlink.Link is WebLink) {
						WebLink webLink = (WebLink)hyperlink.Link;
						return "<a href=\""+ webLink.Url + "\">" + content + "</a>";
					}
					else if(hyperlink.Link is FileLink) {
						FileLink fileLink = (FileLink)hyperlink.Link;
						return "<a href=\"" + fileLink.Url + "\">" + content + "</a>";
					}
					else if(hyperlink.Link is ImageLink) {
						ImageLink imageLink = (ImageLink)hyperlink.Link;
						return "<a href=\""+ imageLink.Url + "\">" + content + "</a>";
					}
					else if(hyperlink.Link is DocumentLink) {
						DocumentLink documentLink = (DocumentLink)hyperlink.Link;
						String url = linkResolver.Resolve(documentLink);
						return "<a " + (linkResolver.GetTitle(documentLink) == null ? "" : "title=\"" + linkResolver.GetTitle(documentLink) + "\" ") + "href=\""+ url+ "\">" + content + "</a>";
					}
				}
				return "<span>" + content + "</span>";
			}

			private String insertSpans(String text, IList<Span> spans, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
				if (spans.Count == 0) {
					return HttpUtility.HtmlEncode(text);
				}

				IDictionary<int, List<Span>> tagsStart = new Dictionary<int, List<Span>>();
				IDictionary<int, List<Span>> tagsEnd = new Dictionary<int, List<Span>>();

				foreach (Span span in spans) {
					if (!tagsStart.ContainsKey(span.Start)) {
						tagsStart.Add(span.Start, new List<Span>());
					}
					if (!tagsEnd.ContainsKey(span.End)) {
						tagsEnd.Add(span.End, new List<Span>());
					}
					tagsStart[span.Start].Add(span);
					tagsEnd[span.End].Add(span);
				}

				char c;
				String html = "";
				Stack<Tuple<Span, String>> stack = new Stack<Tuple<Span, String>>();
				for (int pos = 0, len = text.Length; pos < len; pos++) {
					if (tagsEnd.ContainsKey(pos)) {
						foreach (Span span in tagsEnd[pos]) {
							// Close a tag
							Tuple<Span, String> tag = stack.Pop();
							String innerHtml = serialize(tag.Item1, tag.Item2, linkResolver, htmlSerializer);
							if (stack.Count == 0) {
								// The tag was top level
								html += innerHtml;
							} else {
								// Add the content to the parent tag
								Tuple<Span, String> head = stack.Pop();
								stack.Push(new Tuple<Span, String>(head.Item1, head.Item2 + innerHtml));
							}
						}
					}
					if (tagsStart.ContainsKey(pos)) {
						foreach (Span span in tagsStart[pos]) {
							// Open a tag
							stack.Push(new Tuple<Span, String>(span, ""));
						}
					}
					c = text[pos];
					String escaped = HttpUtility.HtmlEncode(c.ToString());
					if (stack.Count == 0) {
						// Top-level text
						html += escaped;
					} else {
						// Inner text of a span
						Tuple<Span, String> head = stack.Pop();
						stack.Push(new Tuple<Span, String>(head.Item1, head.Item2 + escaped));
					}
				}
				// Close remaining tags
				while (stack.Count > 0) {
					Tuple<Span, String> tag = stack.Pop();
					String innerHtml = serialize(tag.Item1, tag.Item2, linkResolver, htmlSerializer);
					if (stack.Count == 0) {
						// The tag was top level
						html += innerHtml;
					} else {
						// Add the content to the parent tag
						Tuple<Span, String> head = stack.Pop();
						stack.Push(new Tuple<Span, String>(head.Item1, head.Item2 + innerHtml));
					}
				}
				return html;
			}

			public String AsHtml(DocumentLinkResolver linkResolver) {
				return AsHtml(linkResolver, null);
			}

			public String AsHtml(DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) {
				return AsHtml(getBlocks(), linkResolver, htmlSerializer);
			}

		// --

			public static Link ParseLink(JToken json) {
				if (json == null)
					return null;
				String linkType = (string)json["type"];
				JObject value = (JObject)json["value"];
				switch (linkType) {
				case "Link.web":
					return WebLink.Parse(value);
				case "Link.document":
					return DocumentLink.Parse(value);
				case "Link.file":
					return FileLink.Parse(value);
				case "Link.image":
					return ImageLink.Parse(value);
				}
				return null;
			}

			public static Span ParseSpan(JToken json) {
				String type = (string)json["type"];
				int start = (int)json["start"];
				int end = (int)json["end"];
				JToken data = json["data"];

				if (end > start) {
					switch (type) {
					case "strong":
						return new Strong (start, end);
					case "em":
						return new Em (start, end);
					case "hyperlink":
						Link link = ParseLink (data);
						if (link != null) {
							return new Hyperlink (start, end, link);
						}
						break;
					case "label":
						String label = (string)data ["label"];
						return new LabelSpan (start, end, label);
					}
				}

				return null;
			}

			private class ParsedText {
				public String text;
				public IList<Span> spans;

				public ParsedText(String text, IList<Span> spans) {
					this.text = text;
					this.spans = spans;
				}
			}

			static ParsedText ParseText(JToken json) {
				String text = (string)json["text"];
				IList<Span> spans = json["spans"].Select (r => ParseSpan(r)).Where(i => i != null).ToList ();
				return new ParsedText(text, spans);
			}

			static Block ParseBlock(JToken json) {
				String type = (string)json["type"];
				String label = (string)json["label"];
				ParsedText p;
				switch (type) {
				case "heading1":
					p = ParseText(json);
					return new Heading(p.text, p.spans, 1, label);
				case "heading2":
					p = ParseText(json);
					return new Heading(p.text, p.spans, 2, label);
				case "heading3":
					p = ParseText(json);
					return new Heading(p.text, p.spans, 3, label);
				case "heading4":
					p = ParseText(json);
					return new Heading(p.text, p.spans, 4, label);
				case "paragraph":
					p = ParseText(json);
					return new Paragraph(p.text, p.spans, label);
				case "preformatted":
					p = ParseText(json);
					return new Preformatted(p.text, p.spans, label);
				case "list-item":
					p = ParseText(json);
					return new ListItem(p.text, p.spans, false, label);
				case "o-list-item":
					p = ParseText(json);
					return new ListItem(p.text, p.spans, true, label);
				case "image":
					fragments.Image.View view = fragments.Image.View.Parse(json);
					return new Image(view, label);
				case "embed":
					fragments.Embed obj = fragments.Embed.Parse(json);
					return new Embed(obj, label);
				}
				return null;
			}

			public static StructuredText Parse(JToken json) {
				IList<Block> blocks = json.Select (r => ParseBlock(r)).Where(i => i != null).ToList ();
				return new StructuredText(blocks);
			}


		}
	}

}

