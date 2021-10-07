using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using prismic.fragments;

namespace prismic
{
    public abstract class WithFragments
    {
        public IDictionary<string, IFragment> Fragments { get; }

        public WithFragments(IDictionary<string, IFragment> fragments)
        {
            Fragments = fragments;
        }

        public IList<IFragment> GetAll(string field)
        {
            Regex r = new Regex(Regex.Escape(field) + @"\[\d+\]");
            // TODO test this...
            // return Fragments
            //     .Where(f => r.Match(f.Key).Success)
            //     .Select(f => f.Value)
            //     .ToList();
            IList<IFragment> result = new List<IFragment>();
            foreach (KeyValuePair<string, IFragment> entry in Fragments)
            {
                if (r.Match(entry.Key).Success)
                {
                    result.Add(entry.Value);
                }
            }
            return result;
        }

        public IFragment Get(string field)
        {
            if (!Fragments.TryGetValue(field, out IFragment single))
                return null;

            // IList<IFragment> multi = GetAll(field);
            // if (multi.Count > 0)
            // {
            //     return multi[0];
            // }
            return single;
        }

        public string GetText(string field)
        {
            IFragment frag = Get(field);
            if (frag is Text text)
            {
                return text.Value;
            }
            if (frag is Number number)
            {
                return number.Value.ToString();
            }
            if (frag is Color color)
            {
                return color.Hex;
            }
            if (frag is StructuredText sturcturedText)
            {
                var result = "";
                foreach (StructuredText.Block block in sturcturedText.Blocks)
                {
                    if (block is StructuredText.TextBlock textBlock)
                    {
                        result += textBlock.Text;
                    }
                }
                return result;
            }
            if (frag is Number number1)
            {
                return number1.Value.ToString();
            }
            if (frag is BooleanFragment boolean)
            {
                return boolean.Value.ToString();
            }
            return null;
        }

        public Number GetNumber(string field)
        {
            IFragment frag = Get(field);
            return (Number)frag;
        }

        public SliceZone GetSliceZone(string field)
        {
            IFragment frag = Get(field);
            return (SliceZone)frag;
        }

        public Image.View GetImageView(string field, string view)
        {
            var image = GetImage(field);
            if (image != null && image.HasView(view))
                return image.GetView(view);
            return null;
        }

        public Image GetImage(string field)
        {
            IFragment frag = Get(field);
            return frag is Image image ? image : null;
        }

        public ILink GetLink(string field)
        {
            IFragment frag = Get(field);
            return frag is ILink link ? link : null;
        }

        public Date GetDate(string field)
        {
            IFragment frag = Get(field);
            return frag is Date date ? date : null;
        }

        public Timestamp GetTimestamp(string field)
        {
            IFragment frag = Get(field);
            return frag is Timestamp timestamp ? timestamp : null;
        }

        public Embed GetEmbed(string field)
        {
            IFragment frag = Get(field);
            return (Embed)frag;
        }

        public fragments.Group GetGroup(string field)
        {
            IFragment frag = Get(field);
            return (fragments.Group)frag;
        }

        public Color GetColor(string field)
        {
            IFragment frag = Get(field);
            return frag is Color color ? color : null;
        }

        public GeoPoint GetGeoPoint(string field)
        {
            IFragment frag = Get(field);
            return (GeoPoint)frag;
        }

        public StructuredText GetStructuredText(string field)
        {
            IFragment frag = Get(field);
            return (StructuredText)frag;
        }

        public string GetHtml(string field, DocumentLinkResolver resolver)
        {
            return GetHtml(field, resolver, null);
        }

        public string GetHtml(string field, DocumentLinkResolver resolver, HtmlSerializer serializer)
        {
            IFragment fragment = Get(field);
            return GetHtml(fragment, resolver, serializer);
        }

        public static string GetHtml(IFragment fragment, DocumentLinkResolver resolver, HtmlSerializer serializer)
        {
            if (fragment == null)
                return string.Empty;

            switch (fragment)
            {
                case StructuredText structuredText:
                    return structuredText.AsHtml(resolver, serializer);
                case Number number:
                    return number.AsHtml();
                case Color color:
                    return color.AsHtml();
                case Text text:
                    return text.AsHtml();
                case Date date:
                    return date.AsHtml();
                case Embed embed:
                    return embed.AsHtml();
                case Image image:
                    return image.AsHtml(resolver);
                case WebLink webLink:
                    return webLink.AsHtml();
                case DocumentLink docLink:
                    return docLink.AsHtml(resolver);
                case fragments.Group group:
                    return group.AsHtml(resolver);
                case SliceZone zone:
                    return zone.AsHtml(resolver);
                default:
                    return string.Empty;
            }
        }

        public BooleanFragment GetBoolean(string field)
        {
            IFragment fragment = Get(field);
            return (BooleanFragment)fragment;
        }

        public string AsHtml(DocumentLinkResolver linkResolver) => AsHtml(linkResolver, null);

        public string AsHtml(DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer)
        {
            string html = "";
            foreach (KeyValuePair<string, IFragment> fragment in Fragments)
            {
                html += ("<section data-field=\"" + fragment.Key + "\">");
                html += GetHtml(fragment.Key, linkResolver, htmlSerializer);
                html += ("</section>");
            }
            return html.Trim();
        }

        public IList<DocumentLink> LinkedDocuments()
        {
            var result = new List<DocumentLink>();
            foreach (IFragment fragment in Fragments.Values)
            {
                if (fragment is DocumentLink dl)
                {
                    result.Add(dl);
                }
                else if (fragment is StructuredText text)
                {
                    foreach (StructuredText.Block block in text.Blocks)
                    {
                        if (block is StructuredText.TextBlock textBlock)
                        {
                            var spans = textBlock.Spans;
                            foreach (StructuredText.Span span in spans)
                            {
                                if (span is StructuredText.Hyperlink hyperlink)
                                {
                                    var link = hyperlink.Link;
                                    if (link is DocumentLink docLink)
                                    {
                                        result.Add(docLink);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (fragment is fragments.Group group)
                {
                    result.AddRange(group.GroupDocs.SelectMany(d => d.LinkedDocuments()));
                }
            }
            return result;
        }

        public Raw GetRaw(string field)
        {
            var frag = Get(field);
            return (Raw)frag;
        }
    }
}

