﻿﻿﻿using System;

using System.Web;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class Document: WithFragments
	{
		private String id;
		public String Id {
			get {
				return id;
			}
		}
		private String uid;
		public String Uid {
			get {
				return uid;
			}
		}
		private String href;
		public String Href {
			get {
				return href;
			}
		}
		private ISet<String> tags;
		public ISet<String> Tags {
			get {
				return tags;
			}
		}
		private IList<String> slugs;
		public IList<String> Slugs {
			get {
				return slugs;
			}
		}
		public String Slug {
			get {
				if (slugs.Count > 0) {
					return slugs [0];
				} else {
					return "-";
				}
			}
		}
		private String type;
		public String Type {
			get {
				return type;
			}
		}
		private String lang;
		public String Lang {
			get {
				return lang;
			}
		}
		private IList<AlternateLanguage> alternateLanguages;
		public IList<AlternateLanguage> AlternateLanguages {
			get {
				return alternateLanguages;
			}
		}

		public DateTime? FirstPublicationDate { get; }
		public DateTime? LastPublicationDate { get; }

		public Document(string id, string uid, string type, string href, ISet<string> tags, IList<string> slugs, string lang,
			IList<AlternateLanguage> alternateLanguages, IDictionary<string, Fragment> fragments, DateTime? firstPublicationDate, DateTime? lastPublicationDate): base(fragments) {
			this.id = id;
			this.uid = uid;
			this.type = type;
			this.href = href;
			this.tags = tags;
			this.slugs = slugs;
			this.lang = lang;
			this.alternateLanguages = alternateLanguages;
			FirstPublicationDate = firstPublicationDate;
			LastPublicationDate = lastPublicationDate;
		}

		public fragments.DocumentLink AsDocumentLink() {
			return new fragments.DocumentLink(id, uid, type, tags, slugs[0], this.lang, this.Fragments, false);
		}

		public static IDictionary<String, Fragment> parseFragments(JToken json) {
			IDictionary<String, Fragment> fragments = new Dictionary<String, Fragment>();

			if (json == null) {
				return fragments;
			}
			var type = (string)json["type"];


			if (json ["data"] == null) {
				return fragments;
			}
			foreach (KeyValuePair<String, JToken> field in ((JObject)json ["data"][type])) {
				if (field.Value is JArray) {
					var structuredText = prismic.fragments.StructuredText.Parse(field.Value);
					if (structuredText != null) {
						fragments[type + "." + field.Key] = structuredText;
					} else {
						var i = 0;
						foreach (JToken elt in ((JArray)field.Value))
						{
							String fragmentName = type + "." + field.Key + "[" + i++ + "]";
							String fragmentType = (string)elt["type"];
							JToken fragmentValue = elt["value"];
							Fragment fragment = prismic.fragments.FragmentParser.Parse(fragmentType, fragmentValue);
							if (fragment != null)
							{
								fragments[fragmentName] = fragment;
							}
						}
					}
				} else {
					String fragmentName = type + "." + field.Key;
					String fragmentType = (string)field.Value["type"];
					JToken fragmentValue = field.Value["value"];
					Fragment fragment = prismic.fragments.FragmentParser.Parse(fragmentType, fragmentValue);
					if (fragment != null) {
						fragments [fragmentName] = fragment;
					}
				}
			}
			return fragments;
		}

		public static Document Parse(JToken json) {
			var id = (string)json["id"];
			var uid = (string)json["uid"];
			var href = (string)json["href"];
			var type = (string)json["type"];
			var lang = (string)json["lang"];
			var firstPublicationDate = (DateTime?) json["first_publication_date"];
			var lastPublicationDate = (DateTime?)json["last_publication_date"];
			var alternateLanguageJson = json["alternate_languages"] ?? new JArray();

			ISet<String> tags = new HashSet<String>(json ["tags"].Select(r => (string)r));
			IList<String> slugs = json["slugs"].Select(r => HttpUtility.UrlDecode((string)r)).ToList ();
			IList<AlternateLanguage> alternateLanguages = alternateLanguageJson.Select(l => AlternateLanguage.parse(l)).ToList ();
			IDictionary<String, Fragment> frags = parseFragments (json);

			return new Document(id, uid, type, href, tags, slugs, lang, alternateLanguages, frags, firstPublicationDate, lastPublicationDate);
		}


	}

	public class AlternateLanguage
	{
		private String id;
		public String Id {
			get {
				return id;
			}
		}

		private String uid;
		public String UID {
			get {
				return uid;
			}
		}

		private String type;
		public String TYPE {
			get {
				return type;
			}
		}

		private String lang;
		public String LANG {
			get {
				return lang;
			}
		}

		public AlternateLanguage(String id, String uid, String type, String lang) {
			this.id = id;
			this.uid = uid;
			this.type = type;
			this.lang = lang;
		}

		public static AlternateLanguage parse(JToken json) {
			var id = (string)json["id"];
			var uid = (string)json["uid"];
			var type = (string)json["type"];
			var lang = (string)json["lang"];
			return new AlternateLanguage(id, uid, type, lang);
		}
	}
}

