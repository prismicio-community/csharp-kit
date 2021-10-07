namespace prismic
{
    public class PrismicClientException : System.Exception
    {
        public enum ErrorCode
        {
            MALFORMED_URL,
            AUTHORIZATION_NEEDED,
            INVALID_TOKEN,
            UNEXPECTED,
            INVALID_PREVIEW
        }

        public ErrorCode Code { get; }
        public PrismicClientException(ErrorCode code, string message) : base(message)
        {
            Code = code;
        }

        public override string ToString() => $"[{Code}] {base.Message}";

    }
}

