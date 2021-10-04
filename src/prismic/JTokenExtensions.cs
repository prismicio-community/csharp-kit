using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public static class JTokenExtensions
    {
        public static Dictionary<string, TValue> ToDictionary<TValue>(this JToken json, string sourceKey, Func<JToken, TValue> mapFunction)
        {
            var source = (JObject)json[sourceKey];
            var dest = new Dictionary<string, TValue>();

            if (source == null)
                return dest;

            foreach (KeyValuePair<string, JToken> item in source)
            {
                dest.Add(item.Key, mapFunction(item.Value));
            }

            return dest;
        }
    }

}

