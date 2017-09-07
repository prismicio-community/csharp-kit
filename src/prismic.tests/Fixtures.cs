using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace prismic.tests
{
    public class Fixtures
    {
        public static JToken Get(String file)
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = string.Format("{0}{1}fixtures{1}{2}", directory, Path.DirectorySeparatorChar, file);
            string text = System.IO.File.ReadAllText(path);
            return JToken.Parse(text);
        }

        public static Document GetDocument(String file)
        {
            var json = Get(file);
            return Document.Parse(json);
        }
    }        
}
