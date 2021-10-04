using System.Net;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;

namespace prismic
{
    public class Document : WithFragments
    {
        public string Id { get; }
        public string Uid { get; }
        public string Href { get; }
        public ISet<string> Tags { get; }
        public IList<string> Slugs { get; }
        public string Slug => Slugs.Count > 0 ? Slugs[0] : "-";
        public string Type { get; }
        public DateTime? FirstPublicationDate { get; }
        public DateTime? LastPublicationDate { get; }

        public string Lang { get; }
        public IList<AlternateLanguage> AlternateLanguages { get; }

        public Document(string id, string uid, string type, string href, ISet<string> tags, IList<string> slugs, string lang, IList<AlternateLanguage> alternateLanguages, IDictionary<string, IFragment> fragments, DateTime? firstPublicationDate, DateTime? lastPublicationDate)
            : base(fragments)
        {
            Id = id;
            Uid = uid;
            Type = type;
            Href = href;
            Tags = tags;
            Slugs = slugs;
            Lang = lang;
            AlternateLanguages = alternateLanguages;
            FirstPublicationDate = firstPublicationDate;
            LastPublicationDate = lastPublicationDate;
        }

        public fragments.DocumentLink AsDocumentLink() => new fragments.DocumentLink(Id, Uid, Type, Tags, Slugs[0], Lang, Fragments, false);

        public static IDictionary<string, IFragment> ParseFragments(JToken json)
        {
            IDictionary<string, IFragment> fragments = new Dictionary<string, IFragment>();

            if (json == null)
            {
                return fragments;
            }

            var type = (string)json["type"];

            if (json["data"] == null)
            {
                return fragments;
            }

            foreach (var field in (JObject)json["data"][type])
            {
                var fragmentNameBase = $"{type}.{field.Key}";
                var fragmentName = fragmentNameBase;

                if (field.Value is JArray fieldArray)
                {
                    var structuredText = prismic.fragments.StructuredText.Parse(field.Value);
                    if (structuredText != null)
                    {
                        fragments[fragmentName] = structuredText;
                    }
                    else
                    {
                        var i = 0;
                        foreach (JToken elt in fieldArray)
                        {
                            fragmentName = $"{fragmentNameBase}[{i++}]";
                            AddFragment(fragments, fragmentName, MapFragment(elt));
                        }
                    }
                }
                else
                {
                    AddFragment(fragments, fragmentName, MapFragment(field.Value));
                }
            }
            return fragments;
        }

        private static IFragment MapFragment(JToken field)
            => fragments.FragmentParser.Parse((string)field["type"], field["value"]);

        private static void AddFragment(IDictionary<string, IFragment> fragments, string name, IFragment fragment)
        {
            if (fragment == null)
                return;

            fragments[name] = fragment;
        }

        public static Document Parse(JToken json)
        {
            var id = (string)json["id"];
            var uid = (string)json["uid"];
            var href = (string)json["href"];
            var type = (string)json["type"];
            var firstPublicationDate = (DateTime?)json["first_publication_date"];
            var lastPublicationDate = (DateTime?)json["last_publication_date"];
            var lang = (string)json["lang"];
            var alternateLanguageJson = json["alternate_languages"] ?? new JArray();

            var tags = new HashSet<string>(json["tags"].Select(r => (string)r));
            var slugs = json["slugs"].Select(r => WebUtility.UrlDecode((string)r)).ToList();
            IList<AlternateLanguage> alternateLanguages = alternateLanguageJson.Select(l => AlternateLanguage.Parse(l)).ToList();
            var fragments = ParseFragments(json);

            return new Document(id, uid, type, href, tags, slugs, lang, alternateLanguages, fragments, firstPublicationDate, lastPublicationDate);
        }
    }
}
