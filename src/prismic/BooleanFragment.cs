using Newtonsoft.Json.Linq;
using System.Net;

namespace prismic.fragments
{
    public class BooleanFragment : IFragment
    {
        public bool Value { get; set; }

        public BooleanFragment(bool value)
        {
            Value = value;
        }

        public string AsHtml() => $"<span class=\"boolean\">{WebUtility.HtmlEncode(Value.ToString())}</span>";

        public static BooleanFragment Parse(JToken json) => new BooleanFragment((bool)json);
    }

}
