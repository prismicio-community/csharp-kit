using System;

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using prismic.fragments;

namespace prismic
{
	public class GroupDoc: WithFragments
	{
		public GroupDoc (IDictionary<String, Fragment> fragments): base(fragments)
		{
		}

	    public static GroupDoc Parse(JToken json)
	    {
            var fragmentMap = new Dictionary<String, Fragment>();
            foreach (KeyValuePair<String, JToken> field in (JObject)json)
            {
                String fragmentType = (string)field.Value["type"];
                JToken fragmentValue = field.Value["value"];
                Fragment fragment = FragmentParser.Parse(fragmentType, fragmentValue);
                if (fragment != null) fragmentMap[field.Key] = fragment;
            }

            return new GroupDoc(fragmentMap);
        }
	}
}

