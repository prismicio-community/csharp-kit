using System;

namespace prismic
{

	public interface ICache {

	}

	public class NoCache: ICache
	{
		public NoCache ()
		{
		}
	}
}

