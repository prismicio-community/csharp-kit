using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace prismic
{
    internal static class DictionaryExtensions
    {
        internal static string GetQueryString(IDictionary<string, StringValues> values)
        {
            var qb = new QueryBuilder();

            foreach (var value in values)
                qb.Add(value.Key, value.Value.ToString());

            return qb.ToString();
        }
    }
}
