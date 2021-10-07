using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public class ApiData
    {
        public IList<Ref> Refs { get; }
        public IDictionary<string, string> Bookmarks { get; }
        public IDictionary<string, string> Types { get; }
        public IList<string> Tags { get; }
        public IDictionary<string, Form> Forms { get; }
        public string OAuthInitiateEndpoint { get; }
        public string OAuthTokenEndpoint { get; }
        public Experiments Experiments { get; }

        public ApiData(IList<Ref> refs,
            IDictionary<string, string> bookmarks,
            IDictionary<string, string> types,
            IList<string> tags,
            IDictionary<string, Form> forms,
            Experiments experiments,
            string oauthInitiateEndpoint,
            string oauthTokenEndpoint)
        {
            Refs = refs;
            Bookmarks = bookmarks;
            Types = types;
            Tags = tags;
            Forms = forms;
            Experiments = experiments;
            OAuthInitiateEndpoint = oauthInitiateEndpoint;
            OAuthTokenEndpoint = oauthTokenEndpoint;
        }

        public static ApiData Parse(JToken json)
        {
            var refs = json["refs"].Select(r => Ref.Parse((JObject)r)).ToList();
            var tags = json["tags"].Select(r => (string)r).ToList();

            var bookmarks = json.ToDictionary("bookmarks", b => (string)b);
            var types = json.ToDictionary("types", t => (string)t);
            var forms = json.ToDictionary("forms", f => Form.Parse((JObject)f));

            var oauthInitiateEndpoint = (string)json["oauth_initiate"];
            var oauthTokenEndpoint = (string)json["oauth_token"];

            var experiments = Experiments.Parse(json["experiments"]);

            return new ApiData(refs, bookmarks, types, tags, forms, experiments, oauthInitiateEndpoint, oauthTokenEndpoint);
        }
    }

}

