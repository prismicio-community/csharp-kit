using System;

namespace prismic
{
	public interface ILogger
	{
		void log(string level, string msg);
	}

	public class NoLogger: ILogger {
		public NoLogger(){
		}
		public void log(string level, string msg) {}
	}
}

