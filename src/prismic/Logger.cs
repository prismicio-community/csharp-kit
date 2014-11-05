using System;

namespace prismic
{
	public interface ILogger
	{
	}

	public class NoLogger: ILogger {
		public NoLogger(){
		}
	}
}

