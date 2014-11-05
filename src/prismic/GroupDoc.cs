using System;

using System.Collections.Generic;

namespace prismic
{
	public class GroupDoc: WithFragments
	{
		public GroupDoc (IDictionary<String, Fragment> fragments): base(fragments)
		{
		}
	}
}

