using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace prismic
{

    public class Form
    {

        public class Field
        {
            public string Type { get; }
            public bool IsMultiple { get; }
            public string DefaultValue { get; }

            public Field(string type, bool multipl, string defaultValue)
            {
                Type = type;
                IsMultiple = multipl;
                DefaultValue = defaultValue;
            }

            public static Field Parse(JToken json)
            {
                string type = (string)json["type"];
                string defaultValue = json["default"] != null ? (string)json["default"] : null;
                bool multiple = json["multiple"] != null ? (bool)json["multiple"] : false;
                return new Field(type, multiple, defaultValue);
            }

        }

        public string Name { get; }
        public string Method { get; }
        public string Rel { get; }
        public string Enctype { get; }
        public string Action { get; }
        public IDictionary<string, Field> Fields { get; }

        public Form(string name, string method, string rel, string enctype, string action, IDictionary<string, Field> fields)
        {
            Name = name;
            Method = method.ToUpper();
            Rel = rel;
            Enctype = enctype;
            Action = action;
            Fields = new Dictionary<string, Field>(fields);
        }

        public override string ToString() => Method + " " + Action;

        public static Form Parse(JObject json)
        {
            var name = (string)json["name"];
            var method = (string)json["method"];
            var rel = (string)json["rel"];
            var enctype = (string)json["enctype"];
            var action = (string)json["action"];
            var fields = json.ToDictionary("fields", f => Field.Parse((JObject)f));

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
        public class SearchForm
        {
            private readonly PrismicHttpClient _prismicHttpClient;
            private readonly Form form;
            private readonly IDictionary<string, StringValues> data;

            public SearchForm(PrismicHttpClient prismicHttpClient, Form form)
            {
                _prismicHttpClient = prismicHttpClient;
                this.form = form;

                data = form.Fields
                    .Where(entry => entry.Value.DefaultValue != null)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => new StringValues(entry.Value.DefaultValue)
                    );
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
            public SearchForm Set(string field, string value)
            {
                if (value == null)
                    return this;

                if (!form.Fields.TryGetValue(field, out var fieldDesc))
                    throw new ArgumentException("Unknown field", nameof(field));

                if (fieldDesc.IsMultiple)
                {
                    IList<string> existingValue;
                    if (data.ContainsKey(field))
                    {
                        existingValue = data[field];
                    }
                    else
                    {
                        existingValue = new List<string>();
                    }
                    existingValue.Add(value);
                    data[field] = new StringValues(existingValue.ToArray());
                }
                else
                {
                    data[field] = new StringValues(value);
                }
                return this;
            }

            /**
             * A simple helper to set numeric value; see set(String,String).
             * @param field the name of the field to set
             * @param value target value
             * @return the current form, in order to chain those calls
             */
            public SearchForm Set(string field, int value)
            {
                if (!form.Fields.TryGetValue(field, out var fieldDesc))
                    throw new ArgumentException("Unknown field", nameof(field));

                if ("Integer" != fieldDesc.Type)
                    throw new ArgumentException($"Cannot set an Integer value to field {field} of type {fieldDesc.Type}");

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
            public SearchForm Ref(Ref myref) => Ref(myref.Reference);

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
            public SearchForm Ref(string myref) => Set("ref", myref);

            /**
             * Allows to set the size of the pagination of the query's response.
             *
             * The default value is 20; a call with a different page size will look like:
             * <code>api.getForm("everything").pageSize("15").ref(ref).submit();</code>.
             *
             * @param pageSize the size of the pagination you wish
             * @return the current form, in order to chain those calls
             */
            public SearchForm PageSize(string pageSize) => Set("pageSize", pageSize);

            /**
             * Allows to set the size of the pagination of the query's response.
             *
             * The default value is 20; a call with a different page size will look like:
             * <code>api.getForm("everything").pageSize(15).ref(ref).submit();</code>.
             *
             * @param pageSize the size of the pagination you wish
             * @return the current form, in order to chain those calls
             */
            public SearchForm PageSize(int pageSize) => Set("pageSize", pageSize);

            /**
			 * Set the language you want to get for your query.
			 *
			 * @param lang the language code you wish
			 * @return the current form, in order to chain those calls
			 */
            public SearchForm Lang(string lang = null) => Set("lang", lang ?? "*");

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
            public SearchForm Page(string page) => Set("page", page);

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
            public SearchForm Page(int page) => Set("page", page);

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
            public SearchForm Orderings(string orderings) => Set("orderings", orderings);

            /**
             * Start the results after the id passed in parameter. Useful to get the documment following
             * a reference document for example.
             *
             * @param orderings the orderings
             * @return the current form, in order to chain those calls
             */
            public SearchForm Start(string id) => Set("start", id);

            /**
             * Restrict the document fragments to the set of fields specified.
             *
             * @param fields the fields to return
             * @return the current form, in order to chain those calls
             */
            public SearchForm Fetch(params string[] fields)
            {
                if (fields?.Length == 0)
                {
                    return this; // Noop
                }
                else
                {
                    return Set("fetch", string.Join(",", fields));
                }
            }

            /**
             * Include the specified fragment in the details of DocumentLink
             *
             * @param fields the fields to return
             * @return the current form, in order to chain those calls
             */
            public SearchForm FetchLinks(params string[] fields)
            {
                if (fields.Length == 0)
                {
                    return this; // Noop
                }
                else
                {
                    return Set("fetchLinks", String.Join(",", fields));
                }
            }

            // Temporary hack for Backward compatibility
            private string Strip(string q)
            {
                if (q == null)
                    return "";

                var tq = q.Trim();
                if (tq.IndexOf("[") == 0 && tq.LastIndexOf("]") == tq.Length - 1)
                {
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
            public SearchForm Query(string q)
            {
                Field fieldDesc = form.Fields["q"];
                if (fieldDesc != null && fieldDesc.IsMultiple)
                {
                    return Set("q", q);
                }
                else
                {
                    var value = new StringValues("[ " + (form.Fields.ContainsKey("q") ? Strip(form.Fields["q"].DefaultValue) : "") + " " + Strip(q) + " ]");
                    data.Add("q", value);
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
            public SearchForm Query(params IPredicate[] predicates)
            {
                var result = "";
                foreach (Predicate p in predicates)
                {
                    result += p.Q();
                }
                return Query("[" + result + "]");
            }

            /**
             * The method to call to perform and retrieve your query.
             *
             * Please make sure you're set a ref on this SearchForm form before querying, or the kit will complain!
             *
             * @return the list of documents, that can be directly used as such.
             */
            public async Task<Response> Submit()
            {
                if ("GET" != form.Method || "application/x-www-form-urlencoded" != form.Enctype)
                    throw new PrismicClientException(PrismicClientException.ErrorCode.UNEXPECTED, "Form type not supported");

                var url = form.Action;
                var sep = form.Action.Contains("?") ? "&" : "?";
                foreach (KeyValuePair<string, StringValues> d in data)
                {
                    foreach (var v in d.Value)
                    {
                        url += sep;
                        url += d.Key;
                        url += "=";
                        url += WebUtility.UrlEncode(v);
                        sep = "&";
                    }
                }

                return Response.Parse(await _prismicHttpClient.Fetch(url));
            }

            public override string ToString() => DictionaryExtensions.GetQueryString(data);
        }
    }
}
