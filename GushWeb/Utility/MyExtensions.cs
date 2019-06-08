using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Utility
{
    public static class MyUtil
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNullOrDefault(this int? _int)
        {
            return _int == null || _int == 0;
        }

        public static bool IsNullOrDefault(this decimal? _decimal)
        {
            return _decimal == null || _decimal == 0;
        }

        public static int ToInt(this int? _int)
        {
            return _int ?? 0;
        }

        public static decimal ToDecimal(this decimal? _decimal)
        {
            return _decimal ?? 0;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || list.Count() == 0;
        }

        public static string ToYYYYMMDD(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
    }
}