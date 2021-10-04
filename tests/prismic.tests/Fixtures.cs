using Newtonsoft.Json.Linq;
using System.IO;

namespace prismic.AspNetCore.Tests
{
    public class Fixtures
    {
        public static JToken Get(string file)
        {
            var text = GetFileContents(file);
            return JToken.Parse(text);
        }

        public static Document GetDocument(string file)
        {
            var json = Get(file);
            return Document.Parse(json);
        }

        public static string GetFileContents(string file)
        {
            var directory = Directory.GetCurrentDirectory();
            var sep = Path.DirectorySeparatorChar;
            var path = $"{directory}{sep}fixtures{sep}{file}";
            return File.ReadAllText(path);
        }
    }
}
