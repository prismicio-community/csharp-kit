using System;

namespace prismic
{
	public class Error: System.Exception
	{
		public enum ErrorCode {
			MALFORMED_URL,
			AUTHORIZATION_NEEDED, 
			INVALID_TOKEN,
			UNEXPECTED
		}

		private ErrorCode code;
		public ErrorCode Code {
			get {
				return code;
			}
		}

		public Error(ErrorCode code, String message): base(message) {
			this.code = code;
		}

		public override String ToString() {
			return ("[" + code + "] " + base.Message);
		}

	}
}

