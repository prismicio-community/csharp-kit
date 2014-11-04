using System;

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

		private DateTime scheduledAt;
		public DateTime ScheduledAt {
			get {
				return scheduledAt;
			}
		}

		public Ref(String id, String reference, String label, Boolean masterRef, DateTime scheduledAt) {
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
		/*
		static Ref parse(JsonNode json) {
			var id = json.path("id").asText();
			var reference = json.path("ref").asText();
			var label = json.path("label").asText();
			var masterRef = json.path("isMasterRef").asBoolean();
			return new Ref(id, reference, label, masterRef, null);
		}
*/
	}
}

