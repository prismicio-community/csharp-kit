using Newtonsoft.Json.Linq;
using System;
using System.IO;
using NUnit.Framework;

namespace prismic.tests
{
    public class Fixtures
    {
        public static JToken Get(String file)
        {
            var directory = TestContext.CurrentContext.TestDirectory;
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
