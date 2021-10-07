using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;

namespace prismic
{
    namespace fragments
    {
        public class StructuredText : IFragment
        {
            public interface IElement { }

            public abstract class Block : IElement
            {
                public string Label { get; }
                public Block(string label)
                {
                    Label = label;
                }
            }

            public abstract class TextBlock : Block
            {
                public string Text { get; }
                public IList<Span> Spans { get; }
                public TextBlock(string text, IList<Span> spans, string label) : base(label)
                {
                    Text = text;
                    Spans = spans;
                }
            }

            public class Heading : TextBlock
            {
                public int Level { get; }

                public Heading(string text, IList<Span> spans, int level, string label) : base(text, spans, label)
                {
                    Level = level;
                }

            }

            public class Paragraph : TextBlock
            {
                public Paragraph(string text, IList<Span> spans, string label) : base(text, spans, label) { }
            }

            public class Preformatted : TextBlock
            {
                public Preformatted(string text, IList<Span> spans, string label) : base(text, spans, label) { }
            }

            public class ListItem : TextBlock
            {
                public bool IsOrdered { get; }

                public ListItem(string text, IList<Span> spans, bool ordered, string label) : base(text, spans, label)
                {
                    IsOrdered = ordered;
                }
            }

            public class Image : Block
            {
                public fragments.Image.View View { get; }

                public Image(fragments.Image.View view, string label) : base(label)
                {
                    View = view;
                }

                public string Url => View.Url;

                public int Width => View.Width;

                public int Height => View.Height;
            }

            public class Embed : Block
            {
                public fragments.Embed Obj { get; }

                public Embed(fragments.Embed obj, string label) : base(label)
                {
                    Obj = obj;
                }
            }

            public abstract class Span : IElement
            {
                public int Start { get; }
                public int End { get; }
                public Span(int start, int end)
                {
                    Start = start;
                    End = end;
                }
            }

            public class Em : Span
            {
                public Em(int start, int end) : base(start, end) { }
            }

            public class Strong : Span
            {
                public Strong(int start, int end) : base(start, end) { }
            }

            public class Hyperlink : Span
            {
                public ILink Link { get; }

                public Hyperlink(int start, int end, ILink link) : base(start, end)
                {
                    Link = link;
                }
            }

            public class LabelSpan : Span
            {
                public string Label { get; }

                public LabelSpan(int start, int end, string label) : base(start, end)
                {
                    Label = label;
                }
            }

            public IList<Block> Blocks { get; }

            public StructuredText(IList<Block> blocks)
            {
                Blocks = blocks;
            }

            public IList<Block> GetBlocks()
            {
                return Blocks;
            }

            public Heading GetTitle() => FirstOfType<Heading>();

            public Paragraph GetFirstParagraph() => FirstOfType<Paragraph>();

            public Preformatted GetFirstPreformatted() => FirstOfType<Preformatted>();

            public Image GetFirstImage() => FirstOfType<Image>();

            private class BlockGroup
            {
                public string Tag { get; }
                public IList<Block> Blocks { get; }

                public BlockGroup(string tag, IList<Block> blocks)
                {
                    Tag = tag;
                    Blocks = blocks;
                }
            }

            public string AsHtml(IList<Block> blocks, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer)
            {
                var blockGroups = new List<BlockGroup>();
                foreach (Block block in blocks)
                {
                    BlockGroup lastOne = blockGroups.LastOrDefault();
                    var listItem = block as ListItem;
                    var isListItem = listItem != null;
                    var isOrdererdListItem = listItem?.IsOrdered ?? false;

                    if (lastOne != null && "ul" == lastOne.Tag && isListItem && !isOrdererdListItem)
                    {
                        lastOne.Blocks.Add(block);
                    }
                    else if (lastOne != null && "ol" == lastOne.Tag && isListItem && isOrdererdListItem)
                    {
                        lastOne.Blocks.Add(block);
                    }
                    else if (isListItem && !isOrdererdListItem)
                    {
                        BlockGroup newBlockGroup = new BlockGroup("ul", new List<Block>());
                        newBlockGroup.Blocks.Add(block);
                        blockGroups.Add(newBlockGroup);
                    }
                    else if (isListItem && isOrdererdListItem)
                    {
                        BlockGroup newBlockGroup = new BlockGroup("ol", new List<Block>());
                        newBlockGroup.Blocks.Add(block);
                        blockGroups.Add(newBlockGroup);
                    }
                    else
                    {
                        BlockGroup newBlockGroup = new BlockGroup(null, new List<Block>());
                        newBlockGroup.Blocks.Add(block);
                        blockGroups.Add(newBlockGroup);
                    }
                }
                var html = "";
                foreach (BlockGroup blockGroup in blockGroups)
                {
                    if (blockGroup.Tag != null)
                    {
                        html += ("<" + blockGroup.Tag + ">");
                        foreach (Block block in blockGroup.Blocks)
                        {
                            html += (AsHtml(block, linkResolver, htmlSerializer));
                        }
                        html += ("</" + blockGroup.Tag + ">");
                    }
                    else
                    {
                        foreach (Block block in blockGroup.Blocks)
                        {
                            html += (AsHtml(block, linkResolver, htmlSerializer));
                        }
                    }
                }
                return html;
            }

            public string AsHtml(Block block, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer)
            {
                string content = "";
                if (block is Heading)
                {
                    Heading heading = (Heading)block;
                    content = InsertSpans(heading.Text, heading.Spans, linkResolver, htmlSerializer);
                }
                else if (block is Paragraph paragraph)
                {
                    content = InsertSpans(paragraph.Text, paragraph.Spans, linkResolver, htmlSerializer);
                }
                else if (block is Preformatted preformatted)
                {
                    content = InsertSpans(preformatted.Text, preformatted.Spans, linkResolver, htmlSerializer);
                }
                else if (block is ListItem listItem)
                {
                    content = InsertSpans(listItem.Text, listItem.Spans, linkResolver, htmlSerializer);
                }

                if (htmlSerializer != null)
                {
                    string customHtml = htmlSerializer.Serialize(block, content);
                    if (customHtml != null)
                    {
                        return customHtml;
                    }
                }

                string classCode = block.Label == null ? "" : (" class=\"" + block.Label + "\"");
                if (block is Heading)
                {
                    Heading heading = (Heading)block;
                    return "<h" + heading.Level + classCode + ">" + content + "</h" + heading.Level + ">";
                }
                else if (block is Paragraph)
                {
                    return "<p" + classCode + ">" + content + "</p>";
                }
                else if (block is Preformatted)
                {
                    return "<pre" + classCode + ">" + content + "</pre>";
                }
                else if (block is ListItem)
                {
                    return "<li" + classCode + ">" + content + "</li>";
                }
                else if (block is Image image)
                {
                    var labelCode = block.Label == null ? "" : (" " + block.Label);
                    return "<p class=\"block-img" + labelCode + "\">" + image.View.AsHtml(linkResolver) + "</p>";
                }
                else if (block is Embed embed)
                {
                    return embed.Obj.AsHtml();
                }
                return "";
            }

            private static string Serialize(Span span, string content, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer)
            {
                if (htmlSerializer != null)
                {
                    string customHtml = htmlSerializer.Serialize(span, content);
                    if (customHtml != null)
                        return customHtml;
                }

                if (span is Strong)
                {
                    return "<strong>" + content + "</strong>";
                }
                if (span is Em)
                {
                    return "<em>" + content + "</em>";
                }
                if (span is LabelSpan labelSpan)
                {
                    return "<span class=\"" + labelSpan.Label + "\">" + content + "</span>";
                }
                if (span is Hyperlink hyperlink)
                {
                    if (hyperlink.Link is WebLink webLink)
                    {
                        return HtmlExtensions.Link(webLink.Url, content, webLink.Target);
                    }
                    else if (hyperlink.Link is FileLink fileLink)
                    {
                        return HtmlExtensions.Link(fileLink.Url, content);
                    }
                    else if (hyperlink.Link is ImageLink imageLink)
                    {
                        return HtmlExtensions.Link(imageLink.Url, content);
                    }
                    else if (hyperlink.Link is DocumentLink documentLink)
                    {
                        string url = linkResolver.Resolve(documentLink);
                        return HtmlExtensions.Link(url, content, title: linkResolver.GetTitle(documentLink));
                    }
                }
                return "<span>" + content + "</span>";
            }

            private string InsertSpans(string text, IList<Span> spans, DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer)
            {
                if (spans.Count == 0)
                {
                    return WebUtility.HtmlEncode(text);
                }

                IDictionary<int, List<Span>> tagsStart = new Dictionary<int, List<Span>>();
                IDictionary<int, List<Span>> tagsEnd = new Dictionary<int, List<Span>>();

                foreach (Span span in spans)
                {
                    if (!tagsStart.ContainsKey(span.Start))
                    {
                        tagsStart.Add(span.Start, new List<Span>());
                    }
                    if (!tagsEnd.ContainsKey(span.End))
                    {
                        tagsEnd.Add(span.End, new List<Span>());
                    }
                    tagsStart[span.Start].Add(span);
                    tagsEnd[span.End].Add(span);
                }

                char c;
                string html = "";
                Stack<Tuple<Span, string>> stack = new Stack<Tuple<Span, string>>();
                for (int pos = 0, len = text.Length; pos < len; pos++)
                {
                    if (tagsEnd.ContainsKey(pos))
                    {
                        foreach (Span span in tagsEnd[pos])
                        {
                            // Close a tag
                            Tuple<Span, string> tag = stack.Pop();
                            string innerHtml = Serialize(tag.Item1, tag.Item2, linkResolver, htmlSerializer);
                            if (stack.Count == 0)
                            {
                                // The tag was top level
                                html += innerHtml;
                            }
                            else
                            {
                                // Add the content to the parent tag
                                Tuple<Span, string> head = stack.Pop();
                                stack.Push(new Tuple<Span, string>(head.Item1, head.Item2 + innerHtml));
                            }
                        }
                    }
                    if (tagsStart.ContainsKey(pos))
                    {
                        foreach (Span span in tagsStart[pos])
                        {
                            // Open a tag
                            stack.Push(new Tuple<Span, string>(span, ""));
                        }
                    }
                    c = text[pos];
                    
                    string escaped = char.IsSurrogate(c)
                        ? c.ToString()
                        : WebUtility.HtmlEncode(c.ToString());

                    if (stack.Count == 0)
                    {
                        // Top-level text
                        html += escaped;
                    }
                    else
                    {
                        // Inner text of a span
                        Tuple<Span, string> head = stack.Pop();
                        stack.Push(new Tuple<Span, string>(head.Item1, head.Item2 + escaped));
                    }
                }
                // Close remaining tags
                while (stack.Count > 0)
                {
                    Tuple<Span, string> tag = stack.Pop();
                    string innerHtml = Serialize(tag.Item1, tag.Item2, linkResolver, htmlSerializer);
                    if (stack.Count == 0)
                    {
                        // The tag was top level
                        html += innerHtml;
                    }
                    else
                    {
                        // Add the content to the parent tag
                        Tuple<Span, string> head = stack.Pop();
                        stack.Push(new Tuple<Span, string>(head.Item1, head.Item2 + innerHtml));
                    }
                }
                return html;
            }

            public string AsHtml(DocumentLinkResolver linkResolver) => AsHtml(linkResolver, null);

            public string AsHtml(DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer) => AsHtml(GetBlocks(), linkResolver, htmlSerializer);

            // --

            public static ILink ParseLink(JToken json)
            {
                if (json == null)
                    return null;
                var linkType = (string)json["type"];
                var value = (JObject)json["value"];
                switch (linkType)
                {
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

            public static Span ParseSpan(JToken json)
            {
                string type = (string)json["type"];
                int start = (int)json["start"];
                int end = (int)json["end"];
                JToken data = json["data"];

                if (end > start)
                {
                    switch (type)
                    {
                        case "strong":
                            return new Strong(start, end);
                        case "em":
                            return new Em(start, end);
                        case "hyperlink":
                            ILink link = ParseLink(data);
                            if (link != null)
                            {
                                return new Hyperlink(start, end, link);
                            }
                            break;
                        case "label":
                            string label = (string)data["label"];
                            return new LabelSpan(start, end, label);
                    }
                }

                return null;
            }

            private class ParsedText
            {
                public string text;
                public IList<Span> spans;

                public ParsedText(string text, IList<Span> spans)
                {
                    this.text = text;
                    this.spans = spans;
                }
            }

            static ParsedText ParseText(JToken json)
            {
                string text = (string)json["text"];
                IList<Span> spans = json["spans"].Select(r => ParseSpan(r)).Where(i => i != null).ToList();
                return new ParsedText(text, spans);
            }

            static Block ParseBlock(JToken json)
            {
                string type = (string)json["type"];
                string label = (string)json["label"];
                ParsedText p;
                switch (type)
                {
                    case "heading1":
                    case "heading2":
                    case "heading3":
                    case "heading4":
                    case "heading5":
                    case "heading6":
                        p = ParseText(json);
                        var level = int.Parse(type.Replace("heading", string.Empty));
                        return new Heading(p.text, p.spans, level, label);
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

            public static StructuredText Parse(JToken json)
            {
                IList<Block> blocks = json.Select(r => ParseBlock(r)).Where(i => i != null).ToList();
                return new StructuredText(blocks);
            }

            private T FirstOfType<T>() => Blocks.OfType<T>().FirstOrDefault();
        }
    }

}

