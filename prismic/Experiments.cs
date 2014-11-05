using System;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class Experiments {

		private IList<Experiment> draft;
		public IList<Experiment> Draft {
			get { return draft; }
		}
		private IList<Experiment> running;
		public IList<Experiment> Running {
			get { return running; }
		}

		public Experiments(IList<Experiment> draft, IList<Experiment> running) {
			this.draft = draft;
			this.running = running;
		}

		/**
		* All experiments, draft and running
		*/
		public IList<Experiment> getAll() {
			var all = new List<Experiment> ();
			all.AddRange (running.AsEnumerable());
			all.AddRange (draft.AsEnumerable());
			return all;
		}

		public IList<Experiment> getDraft() {
			return draft;
		}

		public IList<Experiment> getRunning() {
			return running;
		}

		/**
   * First running experiment. To be used as the current running experiment
   * null if no running experiment.
   */
		public Experiment getCurrent() {
			if (Running.Count > 0) {
				return Running[0];
			}
			return null;
		}

		/**
   * Get the current running experiment variation ref from a cookie content
   */
		public String refFromCookie(String cookie) {
			if (cookie == null || "" == cookie) {
				return null;
			}
			String[] splitted = cookie.Trim().Split(new string[] { "%20" }, StringSplitOptions.None);
			if (splitted.Length >= 2) {
				Experiment exp = findRunningById(splitted[0]);
				if (exp == null) {
					return null;
				}
				int varIndex = int.Parse(splitted[1]);
				if (varIndex > -1 && varIndex < exp.Variations.Count) {
					return exp.Variations[varIndex].Ref;
				}
			}
			return null;
		}

		public static Experiments Parse(JToken json) {
			if (json == null)
				return null;
			IList<Experiment> draft = json ["draft"].Select (r => Experiment.Parse ((JObject)r)).ToList ();
			IList<Experiment> running = json ["running"].Select (r => Experiment.Parse ((JObject)r)).ToList ();

			return new Experiments(draft, running);
		}

		private Experiment findRunningById(String expId) {
			if (expId == null) return null;
			foreach (Experiment exp in this.running) {
				if (expId == exp.GoogleId) {
					return exp;
				}
			}
			return null;
		}
	}

	public class Experiment {
		private String id;
		public string Id {
			get { return id; }
		}
		private String googleId;
		public string GoogleId {
			get { return googleId; }
		}
		private String name;
		public string Name {
			get { return name; }
		}
		private IList<Variation> variations;
		public IList<Variation> Variations {
			get { return variations; }
		}

		public static String COOKIE_NAME = "io.primic.experiment";

		public Experiment(String id, String googleId, String name, IList<Variation> variations) {
			this.id = id;
			this.googleId = googleId;
			this.name = name;
			this.variations = variations;
		}

		public static Experiment Parse(JObject json) {
			String id = (string)json["id"];
			String googleId = (string)json["googleId"];
			String name = (string)json["name"];

			IList<Variation> variations = json ["variations"].Select (v => Variation.Parse ((JObject)v)).ToList ();

			return new Experiment(id, googleId, name, variations);
		}
	}

	public class Variation {

		private String id;
		public String Id {
			get { return id; }
		}
		private String reference;
		public string Ref {
			get { return reference; }
		}
		private String label;
		public string Label {
			get { return label; }
		}

		public Variation(String id, String reference, String label) {
			this.id = id;
			this.reference = reference;
			this.label = label;
		}

		public static Variation Parse(JObject json) {
			String id = (string)json["id"];
			String reference = (string)json["ref"];
			String label = (string)json["label"];

			return new Variation(id, reference, label);
		}

	}

}

