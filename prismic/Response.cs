using System;
using System.Collections.Generic;

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
		public int getPage() {
			return this.page;
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

		/*
		static Response parse(JsonNode json, FragmentParser fragmentParser) {
			Iterator<JsonNode> resultsJson = null;
			resultsJson = json.path("results").elements();
			List<Document> results = new ArrayList<Document>();
			while (resultsJson.hasNext()) {
				results.add(Document.parse(resultsJson.next(), fragmentParser));
			}
			return new Response(results,
				Integer.parseInt(json.path("page").asText()),
				Integer.parseInt(json.path("results_per_page").asText()),
				Integer.parseInt(json.path("total_results_size").asText()),
				Integer.parseInt(json.path("total_pages").asText()),
				json.path("next_page").asText().equals("null") ? null : json.path("next_page").asText(),
				json.path("prev_page").asText().equals("null") ? null : json.path("prev_page").asText()
			);
		}
*/
	}
}

