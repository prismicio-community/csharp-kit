using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace prismic
{
	public interface IPredicate
	{
		String q();
	}

	public class Predicate: IPredicate {

        private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");
        private String name;
		private String fragment;
		private Object value1;
		private Object value2;
		private Object value3;

		public Predicate(String name, String fragment, Object value1): this(name, fragment, value1, null, null){}

		public Predicate(String name, String fragment, Object value1, Object value2): this(name, fragment, value1, value2, null){}

		public Predicate(String name, String fragment, Object value1, Object value2, Object value3) {
			this.name = name;
			this.fragment = fragment;
			this.value1 = value1;
			this.value2 = value2;
			this.value3 = value3;
		}

		public String q() {
			String result = "[:d = " + this.name + "(";
			if ("similar" == this.name) {
				result += ("\"" + this.fragment + "\"");
			} else {
				result += this.fragment;
			}
			result += ", " + serializeField(value1);
			if (value2 != null) {
				result += ", " + serializeField(value2);
			}
			if (value3 != null) {
				result += ", " + serializeField(value3);
			}
			result += ")]";
			return result;
		}

		private static String serializeField(Object value) {
			if (value is String) {
				return ("\"" + value + "\"");
			} else if (value is System.Collections.IEnumerable) {
				IEnumerable<string> serializedItems = ((System.Collections.IEnumerable)value).Cast<object>().Select( item =>
					serializeField(item)
				);
				return "[" + String.Join(",", serializedItems) + "]";
			} else if (value is Predicates.Month) {
				return ("\"" + capitalize(((Predicates.Month) value).ToString()) + "\"");
			} else if (value is System.DayOfWeek) {
				return ("\"" + capitalize(((System.DayOfWeek) value).ToString()) + "\"");
			} else if (value is DateTime) {
				return (((DateTime) value) - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString(_defaultCultureInfo);
            }
            else if (value is Double)
            {
                return ((Double)value).ToString(_defaultCultureInfo);
            }
            else if (value is Decimal)
            {
                return ((Decimal)value).ToString(_defaultCultureInfo);
            }
            else {
				return value.ToString();
			}
		}

		private static String capitalize(String line) {
			if (line == null || "" == line) return "";
			return line.Substring(0, 1).ToUpper() + line.Substring(1).ToLower();
		}
	}

	public class Predicates {

		public enum Month {
			January, February, March, April, May, June,
			July, August, September, October, November, December
		}

		public static IPredicate at(String fragment, String value) {
			return new Predicate("at", fragment, value);
		}

		public static IPredicate any(String fragment, IEnumerable<String> values) {
			return new Predicate("any", fragment, values);
		}

		public static IPredicate @in(String fragment, IEnumerable<String> values) {
			return new Predicate("in", fragment, values);
		}

		public static IPredicate fulltext(String fragment, String value) {
			return new Predicate("fulltext", fragment, value);
		}

		public static IPredicate similar(String documentId, int maxResults) {
			return new Predicate("similar", documentId, maxResults);
		}

		public static IPredicate lt(String fragment, Double lowerBound) {
			return new Predicate("number.lt", fragment, lowerBound);
		}

		public static IPredicate lt(String fragment, int lowerBound) {
			return new Predicate("number.lt", fragment, lowerBound);
		}

		public static IPredicate gt(String fragment, Double upperBound) {
			return new Predicate("number.gt", fragment, upperBound);
		}

		public static IPredicate gt(String fragment, int upperBound) {
			return new Predicate("number.gt", fragment, (Double)upperBound);
		}

		public static IPredicate inRange(String fragment, int lowerBound, int upperBound) {
			return new Predicate("number.inRange", fragment, (Double)lowerBound, (Double)upperBound);
		}

		public static IPredicate inRange(String fragment, Double lowerBound, Double upperBound) {
			return new Predicate("number.inRange", fragment, lowerBound, upperBound);
		}

		public static IPredicate dateBefore(String fragment, DateTime before) {
			return new Predicate("date.before", fragment, before);
		}

		public static IPredicate dateAfter(String fragment, DateTime after) {
			return new Predicate("date.after", fragment, after);
		}

		public static IPredicate dateBetween(String fragment, DateTime lower, DateTime upper) {
			return new Predicate("date.between", fragment, lower, upper);
		}

		public static IPredicate dayOfMonth(String fragment, int day) {
			return new Predicate("date.day-of-month", fragment, day);
		}

		public static IPredicate dayOfMonthBefore(String fragment, int day) {
			return new Predicate("date.day-of-month-before", fragment, day);
		}

		public static IPredicate dayOfMonthAfter(String fragment, int day) {
			return new Predicate("date.day-of-month-after", fragment, day);
		}

		public static IPredicate dayOfWeek(String fragment, DayOfWeek day) {
			return new Predicate("date.day-of-week", fragment, day);
		}

		public static IPredicate dayOfWeekAfter(String fragment, DayOfWeek day) {
			return new Predicate("date.day-of-week-after", fragment, day);
		}

		public static IPredicate dayOfWeekBefore(String fragment, DayOfWeek day) {
			return new Predicate("date.day-of-week-before", fragment, day);
		}

		public static IPredicate month(String fragment, Month month) {
			return new Predicate("date.month", fragment, month);
		}

		public static IPredicate monthBefore(String fragment, Month month) {
			return new Predicate("date.month-before", fragment, month);
		}

		public static IPredicate monthAfter(String fragment, Month month) {
			return new Predicate("date.month-after", fragment, month);
		}

		public static IPredicate year(String fragment, int year) {
			return new Predicate("date.year", fragment, year);
		}

		public static IPredicate hour(String fragment, int hour) {
			return new Predicate("date.hour", fragment, hour);
		}

		public static IPredicate hourBefore(String fragment, int hour) {
			return new Predicate("date.hour-before", fragment, hour);
		}

		public static IPredicate hourAfter(String fragment, int hour) {
			return new Predicate("date.hour-after", fragment, hour);
		}

		public static IPredicate near(String fragment, Double latitude, Double longitude, int radius) {
			return new Predicate("geopoint.near", fragment, latitude, longitude, radius);
		}

	}
}

