using System;

using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace prismic
{

	public class Form {

		public class Field {

			private String typ;
			public String Type {
				get {
					return typ;
				}
			}

			private Boolean multipl;
			public Boolean IsMultiple {
				get {
					return multipl;
				}
			}

			private String defaultValue;
			public String DefaultValue {
				get {
					return defaultValue;
				}
			}

			public Field(String typ, Boolean multipl, String defaultValue) {
				this.typ = typ;
				this.multipl = multipl;
				this.defaultValue = defaultValue;
			}

			// --

			public static Field Parse(JToken json) {
				String type = (string)json["type"];
				String defaultValue = (json["default"] != null ? (string)json["default"] : null);
				Boolean multiple = (json["multiple"] != null ? (Boolean)json["multiple"] : false);
				return new Field(type, multiple, defaultValue);
			}

		}

		// --

		private String name;
		public String Name {
			get {
				return name;
			}
		}

		private String method;
		public String Method {
			get {
				return method;
			}
		}

		private String rel;
		public String Rel {
			get {
				return rel;
			}
		}

		private String enctype;
		public String Enctype {
			get {
				return enctype;
			}
		}

		private String action;
		public String Action {
			get {
				return action;
			}
		}

		private IDictionary<String,Field> fields;
		public IDictionary<String,Field> Fields {
			get {
				return fields;
			}
		}

		public Form(String name, String method, String rel, String enctype, String action, IDictionary<String,Field> fields) {
			this.name = name;
			this.method = method.ToUpper();
			this.rel = rel;
			this.enctype = enctype;
			this.action = action;
			this.fields = new Dictionary<String, Field>(fields);
		}

		public String toString() {
			return method + " " + action;
		}

		// --

		public static Form Parse(JObject json) {
			String name = (string)json["name"];
			String method = (string)json["method"];
			String rel = (string)json["rel"];
			String enctype = (string)json["enctype"];
			String action = (string)json["action"];

			var fields = new Dictionary<String,Field>();
			foreach (KeyValuePair<String, JToken> t in ((JObject)json ["fields"])) {
				fields [t.Key] = Field.Parse((JObject)t.Value);
			}

			return new Form(name, method, rel, enctype, action, fields);
		}

/**
   * The object you will use to perform queries. At the moment, only queries of the type "SearchForm" exist in prismic.io's APIs.
   * There is one named "everything", that allow to query through the while repository, and there is also one per collection
   * created by prismic.io administrators in the writing-room.
   *
   * From an {@link Api} object, you get a SearchForm form like this: <code>api.getForm("everything");</code>
   *
   * Then, from a SearchForm form, you query like this: <code>search.query("[[:d = at(document.type, "Product")]]").ref(ref).submit();</code>
   */
		public class SearchForm {

			private Api api;
			private Form form;
			private IDictionary<String,IList<String>> data;

			public SearchForm(Api api, Form form) {
				this.api = api;
				this.form = form;
				this.data = new Dictionary<String,IList<String>>();
				foreach(KeyValuePair<String,Field> entry in form.Fields) {
					if (entry.Value.DefaultValue != null) {
						IList<String> value = new List<String>();
						value.Add(entry.Value.DefaultValue);
						this.data[entry.Key] = value;
					}
				}
			}

			/**
     * Allows to set one of the form's fields, such as "q" for the query field, or the "ordering" field, or the "pageSize" field.
     * The field must exist in the RESTful description that is in the /api document. To be on the safe side, you should use the
     * specialized methods, and use <code>searchForm.orderings(o)</code> rather than <code>searchForm.set("orderings", o)</code>
     * if they exist.
     *
     * @param field the name of the field to set
     * @param value the value with which to set it
     * @return the current form, in order to chain those calls
     */
			public SearchForm Set(String field, String value) {
				if (value == null) {
					// null value, do nothing
					return this;
				}
				Field fieldDesc = form.Fields[field];
				if(fieldDesc == null) {
					throw new ArgumentException("Unknown field " + field); 
				}
				if(fieldDesc.IsMultiple) {
					IList<String> existingValue;
					if (data.ContainsKey(field)) {
						existingValue = data[field];
					} else {
						existingValue = new List<String>();
					}
					existingValue.Add(value);
					data[field] = existingValue;
				} else {
					var newValue = new List<String>();
					newValue.Add(value);
					data[field] = newValue;
				}
				return this;
			}

            /**
             * A simple helper to set numeric value; see set(String,String).
             * @param field the name of the field to set
             * @param value target value
             * @return the current form, in order to chain those calls
             */
            public SearchForm Set(String field, int value)
            {
                Field fieldDesc = form.Fields[field];
                if (fieldDesc == null)
                {
                    throw new ArgumentException("Unknown field " + field);
                }
                if ("Integer" != fieldDesc.Type)
                {
                    throw new ArgumentException("Cannot set an Integer value to field " + field + " of type " + fieldDesc.Type);
                }
                return Set(field, value.ToString());
            }

			/**
     * Allows to set the ref on which you wish to be performing the query.
     *
     * This is mandatory to submit a query; if you call <code>api.getForm("everything").submit();</code>, the kit will complain!
     *
     * Please do not forge the ref dynamically in this call, like this: <code>ref(api.getMaster())</code>.
     * Prefer to set a ref variable once for your whole webpage, and use that variable in this method: <code>ref(ref)</code>.
     * That way, you can change this variable's assignment once, and trivially set your whole webpage into the future or the past.
     *
     * @param ref the ref object representing the ref on which you wish to query
     * @return the current form, in order to chain those calls
     */
			public SearchForm Ref(Ref myref) {
				return Ref(myref.Reference);
			}

			/**
     * Allows to set the ref on which you wish to be performing the query.
     *
     * This is mandatory to submit a query; if you call <code>api.getForm("everything").submit();</code>, the kit will complain!
     *
     * Please do not forge the ref dynamically in this call, like this: <code>ref(api.getMaster().getRef())</code>.
     * Prefer to set a ref variable once for your whole webpage, and use that variable in this method: <code>ref(ref)</code>.
     * That way, you can change this variable's assignment once, and trivially set your whole webpage into the future or the past.
     *
     * @param ref the ID of the ref on which you wish to query
     * @return the current form, in order to chain those calls
     */
			public SearchForm Ref(String myref) {
				return Set("ref", myref);
			}

			/**
     * Allows to set the size of the pagination of the query's response.
     *
     * The default value is 20; a call with a different page size will look like:
     * <code>api.getForm("everything").pageSize("15").ref(ref).submit();</code>.
     *
     * @param pageSize the size of the pagination you wish
     * @return the current form, in order to chain those calls
     */
			public SearchForm PageSize(String pageSize) {
				return Set("pageSize", pageSize);
			}

			/**
     * Allows to set the size of the pagination of the query's response.
     *
     * The default value is 20; a call with a different page size will look like:
     * <code>api.getForm("everything").pageSize(15).ref(ref).submit();</code>.
     *
     * @param pageSize the size of the pagination you wish
     * @return the current form, in order to chain those calls
     */
			public SearchForm PageSize(int pageSize) {
				return Set("pageSize", pageSize);
			}

			/**
     * Allows to set which page you want to get for your query.
     *
     * The default value is 1; a call for a different page will look like:
     * <code>api.getForm("everything").page("2").ref(ref).submit();</code>
     * (do remember that the default size of a page is 20, you can change it with <code>pageSize</code>)
     *
     * @param page the page number
     * @return the current form, in order to chain those calls
     */
			public SearchForm Page(String page) {
				return Set("page", page);
			}

			/**
     * Allows to set which page you want to get for your query.
     *
     * The default value is 1; a call for a different page will look like:
     * <code>api.getForm("everything").page(2).ref(ref).submit();</code>
     * (do remember that the default size of a page is 20, you can change it with <code>pageSize</code>)
     *
     * @param page the page number
     * @return the current form, in order to chain those calls
     */
			public SearchForm Page(int page) {
				return Set("page", page);
			}

			/**
     * Allows to set which ordering you want for your query.
     *
     * A call will look like:
     * <code>api.getForm("products").orderings("[my.product.price]").ref(ref).submit();</code>
     * Read prismic.io's API documentation to learn more about how to write orderings.
     *
     * @param orderings the orderings
     * @return the current form, in order to chain those calls
     */
			public SearchForm Orderings(String orderings) {
				return Set("orderings", orderings);
			}

			/**
     * Start the results after the id passed in parameter. Useful to get the documment following
     * a reference document for example.
     *
     * @param orderings the orderings
     * @return the current form, in order to chain those calls
     */
			public SearchForm Start(String id) {
				return Set ("start", id);
			}

			/**
     * Restrict the document fragments to the set of fields specified.
     *
     * @param fields the fields to return
     * @return the current form, in order to chain those calls
     */
			public SearchForm Fetch(params String[] fields) {
				if (fields.Length == 0) {
					return this; // Noop
				} else {
					return Set("fetch", String.Join(",", fields));
				}
			}

			/**
     * Include the specified fragment in the details of DocumentLink
     *
     * @param fields the fields to return
     * @return the current form, in order to chain those calls
     */
			public SearchForm FetchLinks(params String[] fields) {
				if (fields.Length == 0) {
					return this; // Noop
				} else {
					return Set("fetchLinks", String.Join(",", fields));
				}
			}

			// Temporary hack for Backward compatibility
			private String strip(String q) {
				if(q == null) return "";
				String tq = q.Trim();
				if(tq.IndexOf("[") == 0 && tq.LastIndexOf("]") == tq.Length - 1) {
					return tq.Substring(1, tq.Length - 1);
				}
				return tq;
			}

			/**
     * Allows to set the query field of the current form. For instance:
     * <code>search.query("[[:d = at(document.type, "Product")]]");</code>
     * Look up prismic.io's documentation online to discover the possible query predicates.
     *
     * Beware: a query is a list of predicates, therefore, it always starts with "[[" and ends with "]]".
     *
     * @param q the query to pass
     * @return the current form, in order to chain those calls
     */
			public SearchForm Query(String q) {
				Field fieldDesc = form.Fields["q"];
				if(fieldDesc != null && fieldDesc.IsMultiple) {
					return Set("q", q);
				} else {
					var value = new List<String>();
					value.Add(("[ " + (form.Fields.ContainsKey("q") ? strip(form.Fields["q"].DefaultValue) : "") + " " + strip(q) + " ]"));
					this.data.Add("q", value);
					return this;
				}
			}

			/**
     * Allows to set the query field of the current form, using Predicate objects. Example:
     * <code>search.query(Predicates.at("document.type", "Product"));</code>
     * See io.prismic.Predicates for more helper methods.
     *
     * @param predicates any number of predicate, is more than one is provided documents that satisfy all predicates will be returned ("AND" query)
     * @return the current form, in order to chain those calls
     */
			public SearchForm Query(params IPredicate[] predicates) {
				String result = "";
				foreach (Predicate p in predicates) {
					result += p.q();
				}
				return this.Query("[" + result + "]");
			}

			/**
     * The method to call to perform and retrieve your query.
     *
     * Please make sure you're set a ref on this SearchForm form before querying, or the kit will complain!
     *
     * @return the list of documents, that can be directly used as such.
     */
			public async Task<Response> Submit() {
				if("GET" == form.Method && "application/x-www-form-urlencoded" == form.Enctype) {
					var url = form.Action;
					var sep = form.Action.Contains("?") ? "&" : "?";
					foreach(KeyValuePair<String,IList<String>> d in data) {
						foreach(String v in d.Value) {
							url += sep;
							url += d.Key;
							url += "=";
							url += HttpUtility.UrlEncode(v);
							sep = "&";
						}
					}
					api.Logger.log ("DEBUG", "Fetching URL: " + url);
					Console.WriteLine ("Fetching URL: " + url);
					var json = await api.PrismicHttpClient.fetch (url, api.Logger, api.Cache);
					return Response.Parse(json);
				} else {
					throw new Error(Error.ErrorCode.UNEXPECTED, "Form type not supported");
				}
			}

			public String toString() {
				String dataStr = "";
				foreach(KeyValuePair<String,IList<String>> d in data) {
					foreach(String v in d.Value) {
						dataStr += d.Key + "=" + v + " ";
					}
				}
				return form.ToString() + " {" + dataStr.Trim() + "}";
			}

		}

	}

}

