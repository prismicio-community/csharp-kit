using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace prismic.tests
{
    [TestClass]
    public class SliceZoneParsingTests
    {

        [TestMethod]
        public async Task SliceZoneAccess()
        {
            var api = await Api.Get("https://primsic-mapping-integration-tests.prismic.io/api");

            var response = await api.Form("everything")
                .Ref(api.Master.Reference)
                .Query(Predicates.at("document.type", "new_slice_type_mapping"))
                .Submit();

            var document = response.Results.FirstOrDefault();
            var html = document.AsHtml(DocumentLinkResolver.For(_ => string.Empty));
            Assert.AreNotEqual(html, string.Empty);
        }
    }
}