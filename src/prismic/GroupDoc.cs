using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using prismic.fragments;

namespace prismic
{
    public class GroupDoc: WithFragments
	{
		public GroupDoc (IDictionary<string, IFragment> fragments): base(fragments)
		{
		}

	    public static GroupDoc Parse(JToken json)
	    {
            var fragmentMap = new Dictionary<string, IFragment>();
            foreach (KeyValuePair<string, JToken> field in (JObject)json)
            {
                // TODO chance to refactor fragment parsing...
                string fragmentType = (string)field.Value["type"];
                JToken fragmentValue = field.Value["value"];
                IFragment fragment = FragmentParser.Parse(fragmentType, fragmentValue);
                
                if (fragment != null) 
                    fragmentMap[field.Key] = fragment;
            }

            return new GroupDoc(fragmentMap);
        }
	}
}

