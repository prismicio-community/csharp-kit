using System;

using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
	public class Ref
	{
		private String id;
		public String Id {
			get {
				return id;
			}
		}
			
		private String reference;
		public String Reference {
			get {
				return reference;
			}
		}

		private String label;
		public String Label {
			get {
				return label;
			}
		}

		private Boolean masterRef;
		public Boolean IsMasterRef {
			get {
				return masterRef;
			}
		}

		private DateTime? scheduledAt;
		public DateTime? ScheduledAt {
			get {
				return scheduledAt;
			}
		}

		public Ref(String id, String reference, String label, Boolean masterRef, DateTime? scheduledAt) {
			this.id = id;
			this.reference = reference;
			this.label = label;
			this.masterRef = masterRef;
			this.scheduledAt = scheduledAt;
		}

		public override String ToString() {
			return ("Ref: " + reference + (label != null ? " (" + label + ")" : ""));
		}

		// --

		public static Ref Parse(JObject json) {
			var id = (string)json ["id"];
			var reference = (string)json["ref"];
			var label = (string)json["label"];
			var masterRef = json["isMasterRef"] != null && (Boolean)json["isMasterRef"];
			return new Ref(id, reference, label, masterRef, null);
		}

	}
}

