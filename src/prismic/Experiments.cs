using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public class Experiments
    {
        public IList<Experiment> Draft { get; }
        public IList<Experiment> Running { get; }

        public Experiments(IList<Experiment> draft, IList<Experiment> running)
        {
            Draft = draft;
            Running = running;

        }

        /**
		* All experiments, draft and running
		*/
        public IList<Experiment> GetAll()
        {
            var all = new List<Experiment>();
            all.AddRange(Running.AsEnumerable());
            all.AddRange(Draft.AsEnumerable());
            return all;
        }

        public IList<Experiment> GetDraft() => Draft;

        public IList<Experiment> GetRunning() => Running;

        /**
		* First running experiment. To be used as the current running experiment
		* null if no running experiment.
		*/
        public Experiment GetCurrent()
        {
            if (Running.Count > 0)
            {
                return Running[0];
            }
            return null;
        }

        /**
		* Get the current running experiment variation ref from a cookie content
		*/
        public String RefFromCookie(String cookie)
        {
            if (cookie == null || "" == cookie)
            {
                return null;
            }
            var split = cookie.Trim().Split(new string[] { "%20" }, StringSplitOptions.None);
            if (split.Length >= 2)
            {
                Experiment exp = FindRunningById(split[0]);
                if (exp == null)
                {
                    return null;
                }
                var varIndex = int.Parse(split[1]);
                if (varIndex > -1 && varIndex < exp.Variations.Count)
                {
                    return exp.Variations[varIndex].Ref;
                }
            }
            return null;
        }

        public static Experiments Parse(JToken json)
        {
            if (json == null)
                return null;

            var draft = json["draft"].Select(r => Experiment.Parse((JObject)r)).ToList();
            var running = json["running"].Select(r => Experiment.Parse((JObject)r)).ToList();

            return new Experiments(draft, running);
        }

        private Experiment FindRunningById(String expId)
        {
            if (expId == null)
                return null;

            foreach (Experiment exp in Running)
            {
                if (expId == exp.GoogleId)
                {
                    return exp;
                }
            }

            return null;
        }
    }

    public class Experiment
    {
        public string Id { get; }
        public string GoogleId { get; }
        public string Name { get; }
        public IList<Variation> Variations { get; }
        public static string COOKIE_NAME = "io.prismic.experiment";

        public Experiment(string id, string googleId, string name, IList<Variation> variations)
        {
            Id = id;
            GoogleId = googleId;
            Name = name;
            Variations = variations;
        }

        public static Experiment Parse(JObject json)
        {
            var id = (string)json["id"];
            var googleId = (string)json["googleId"];
            var name = (string)json["name"];

            IList<Variation> variations = json["variations"].Select(v => Variation.Parse((JObject)v)).ToList();

            return new Experiment(id, googleId, name, variations);
        }
    }

    public class Variation
    {
        public string Id { get; }
        public string Ref { get; }
        public string Label { get; }

        public Variation(string id, string reference, string label)
        {
            Id = id;
            Ref = reference;
            Label = label;
        }

        public static Variation Parse(JObject json)
        {
            string id = (string)json["id"];
            string reference = (string)json["ref"];
            string label = (string)json["label"];

            return new Variation(id, reference, label);
        }
    }
}

