using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public class Response
	{
        public IList<Document> Results { get; }
        public int Page { get; }
        public int ResultsPerPage { get; }
        public int TotalResultsSize { get; }
        public int TotalPages { get; }
        public string NextPage { get; }
        public string PrevPage { get; }

        public Response(IList<Document> results, int page, int results_per_page, int total_results_size, int total_pages, string next_page, string prev_page){
			Results = results;
			Page = page;
			ResultsPerPage = results_per_page;
			TotalResultsSize = total_results_size;
			TotalPages = total_pages;
			NextPage = next_page;
			PrevPage = prev_page;
		}

		public static Response Parse(JToken json) {
			IList<Document> results = json ["results"].Select (r => Document.Parse ((JObject)r)).ToList ();

			return new Response(results,
				int.Parse((string)json["page"]),
				int.Parse((string)json["results_per_page"]),
				int.Parse((string)json["total_results_size"]),
				int.Parse((string)json["total_pages"]),
				(json["next_page"] != null ? (string)json["next_page"] : null),
				(json["prev_page"] != null ? (string)json["prev_page"] : null)
			);
		}

	}
}

