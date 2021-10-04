using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace prismic
{
    public interface IPredicate
    {
        string Q();
    }

    public class Predicate : IPredicate
    {

        private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");
        private readonly string Name;
        private readonly string Fragment;
        private readonly object Value1;
        private readonly object Value2;
        private readonly object Value3;

        public Predicate(string name, string path) : this(name, path, null, null, null) { }

        public Predicate(string name, string fragment, object value1) : this(name, fragment, value1, null, null) { }

        public Predicate(string name, string fragment, object value1, object value2) : this(name, fragment, value1, value2, null) { }

        public Predicate(string name, string fragment, object value1, object value2, object value3)
        {
            Name = name;
            Fragment = fragment;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        public string Q()
        {
            string result = "[:d = " + Name + "(";
            if ("similar" == Name)
            {
                result += ("\"" + Fragment + "\"");
            }
            else
            {
                result += Fragment;
            }

            var values = string.Join(
                ", ",
                new[] { Value1, Value2, Value3 }
                    .Where(x => x != null)
                    .Select(SerializeField)
                    .ToList()
            );

            if (!string.IsNullOrWhiteSpace(values))
                result += $", {values}";

            result += ")]";
            return result;
        }

        private static string SerializeField(object value)
        {
            switch (value)
            {
                case string s:
                    return $"\"{value}\"";

                case IEnumerable items:
                    var serializedItems = items.Cast<object>().Select(item => SerializeField(item));
                    return $"[{string.Join(",", serializedItems)}]";

                case Predicates.Months months:
                    return $"\"{Capitalize(months.ToString())}\"";

                case DayOfWeek day:
                    return $"\"{Capitalize(day.ToString())}\"";

                case DateTime dt:
                    return (dt - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString(_defaultCultureInfo);

                case double d:
                    return d.ToString(_defaultCultureInfo);

                case decimal d:
                    return d.ToString(_defaultCultureInfo);

                default:
                    return value.ToString();
            }
        }

        private static string Capitalize(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return "";

            return line.Substring(0, 1).ToUpper() + line.Substring(1).ToLower();
        }
    }

    public static class Predicates
    {

        public enum Months
        {
            January, February, March, April, May, June,
            July, August, September, October, November, December
        }

        public static IPredicate At(string fragment, string value) => new Predicate("at", fragment, value);

        public static IPredicate At(string fragment, string[] values) => new Predicate("at", fragment, values);

        public static IPredicate Any(string fragment, IEnumerable<string> values) => new Predicate("any", fragment, values);

        public static IPredicate In(string fragment, IEnumerable<string> values) => new Predicate("in", fragment, values);

        public static IPredicate Fulltext(string fragment, string value) => new Predicate("fulltext", fragment, value);

        public static IPredicate Similar(string documentId, int maxResults) => new Predicate("similar", documentId, maxResults);

        public static IPredicate Lt(string fragment, double lowerBound) => new Predicate("number.lt", fragment, lowerBound);

        public static IPredicate Lt(string fragment, int lowerBound) => new Predicate("number.lt", fragment, lowerBound);

        public static IPredicate Gt(string fragment, double upperBound) => new Predicate("number.gt", fragment, upperBound);

        public static IPredicate Gt(string fragment, int upperBound) => new Predicate("number.gt", fragment, (double)upperBound);

        public static IPredicate InRange(string fragment, int lowerBound, int upperBound) => new Predicate("number.inRange", fragment, (double)lowerBound, (double)upperBound);

        public static IPredicate InRange(string fragment, double lowerBound, double upperBound) => new Predicate("number.inRange", fragment, lowerBound, upperBound);

        public static IPredicate DateBefore(string fragment, DateTime before) => new Predicate("date.before", fragment, before);

        public static IPredicate DateAfter(string fragment, DateTime after) => new Predicate("date.after", fragment, after);

        public static IPredicate DateBetween(string fragment, DateTime lower, DateTime upper) => new Predicate("date.between", fragment, lower, upper);

        public static IPredicate DayOfMonth(string fragment, int day) => new Predicate("date.day-of-month", fragment, day);

        public static IPredicate DayOfMonthBefore(string fragment, int day) => new Predicate("date.day-of-month-before", fragment, day);

        public static IPredicate DayOfMonthAfter(string fragment, int day) => new Predicate("date.day-of-month-after", fragment, day);

        public static IPredicate DayOfWeek(string fragment, DayOfWeek day) => new Predicate("date.day-of-week", fragment, day);

        public static IPredicate DayOfWeekAfter(string fragment, DayOfWeek day) => new Predicate("date.day-of-week-after", fragment, day);

        public static IPredicate DayOfWeekBefore(string fragment, DayOfWeek day) => new Predicate("date.day-of-week-before", fragment, day);

        public static IPredicate Month(string fragment, Months month) => new Predicate("date.month", fragment, month);

        public static IPredicate MonthBefore(string fragment, Months month) => new Predicate("date.month-before", fragment, month);

        public static IPredicate MonthAfter(string fragment, Months month) => new Predicate("date.month-after", fragment, month);

        public static IPredicate Year(string fragment, int year) => new Predicate("date.year", fragment, year);

        public static IPredicate Hour(string fragment, int hour) => new Predicate("date.hour", fragment, hour);

        public static IPredicate HourBefore(string fragment, int hour) => new Predicate("date.hour-before", fragment, hour);

        public static IPredicate HourAfter(string fragment, int hour) => new Predicate("date.hour-after", fragment, hour);

        public static IPredicate Near(string fragment, double latitude, double longitude, int radius) => new Predicate("geopoint.near", fragment, latitude, longitude, radius);

        public static IPredicate Not(string fragment, string value) => new Predicate("not", fragment, value);

        public static IPredicate Not(string fragment, string[] values) => new Predicate("not", fragment, values);

        public static IPredicate Has(string path) => new Predicate("has", path);
    }
}

