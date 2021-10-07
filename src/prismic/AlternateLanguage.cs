using Newtonsoft.Json.Linq;

namespace prismic
{
    public class AlternateLanguage
    {
        public string Id { get; }
        public string UID { get; }
        public string Type { get; }
        public string Lang { get; }

        public AlternateLanguage(string id, string uid, string type, string lang)
        {
            Id = id;
            UID = uid;
            Type = type;
            Lang = lang;
        }

        public static AlternateLanguage Parse(JToken json)
        {
            var id = (string)json["id"];
            var uid = (string)json["uid"];
            var type = (string)json["type"];
            var lang = (string)json["lang"];
            return new AlternateLanguage(id, uid, type, lang);
        }
    }
}

