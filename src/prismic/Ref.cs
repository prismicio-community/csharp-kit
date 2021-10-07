using System;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public class Ref
    {
        public string Id { get; }
        public string Reference { get; }
        public string Label { get; }
        public bool IsMasterRef { get; }
        public DateTime? ScheduledAt { get; }

        public Ref(string id, string reference, string label, bool masterRef, DateTime? scheduledAt)
        {
            Id = id;
            Reference = reference;
            Label = label;
            IsMasterRef = masterRef;
            ScheduledAt = scheduledAt;
        }

        public override string ToString()
        {
            return ("Ref: " + Reference + (Label != null ? " (" + Label + ")" : ""));
        }

        public static Ref Parse(JObject json)
        {
            var id = (string)json["id"];
            var reference = (string)json["ref"];
            var label = (string)json["label"];
            var masterRef = json["isMasterRef"] != null && (bool)json["isMasterRef"];

            DateTime? scheduledAt = null;
            if (DateTime.TryParse(json["scheduledAt"]?.ToString(), out var scheduledAtParsed))
            {
                scheduledAt = scheduledAtParsed;
            }

            return new Ref(id, reference, label, masterRef, scheduledAt);
        }

    }
}

