using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class Response
	{
		private IList<Document> results;
		public IList<Document> Results {
			get {
				return results;
			}
		}
		private int page;
		public int Page {
			get {
				return this.page;
			}
		}

		private int results_per_page;
		public int ResultsPerPage {
			get {
				return this.results_per_page;
			}
		}

		private int total_results_size;
		public int TotalResultsSize {
			get {
				return this.total_results_size;
			}
		}

		private int total_pages;
		public int TotalPages {
			get {
				return this.total_pages;
			}
		}

		private String next_page;
		public String NextPage {
			get {
				return this.next_page;
			}
		}

		private String prev_page;
		public String PrevPage {
			get {
				return this.prev_page;
			}
		}

		public Response(IList<Document> results, int page, int results_per_page, int total_results_size, int total_pages, String next_page, String prev_page){
			this.results = results;
			this.page = page;
			this.results_per_page = results_per_page;
			this.total_results_size = total_results_size;
			this.total_pages = total_pages;
			this.next_page = next_page;
			this.prev_page = prev_page;
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

