using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

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
            return list == null || !list.Any();
        }

        public static string ToYYYYMMDD(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static string ToSalt(this string value, string salt)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(salt + value, "SHA1");
        }

        public static int IndexOf<T>(this T?[] zstr, T?[] mstr) where T : struct
        {
            int i, j;
            int[] next = new int[mstr.Length];
            GetNextVal(mstr, next);
            i = 0;
            j = 0;
            while (i < zstr.Length && j < mstr.Length)
            {
                if (j == -1 || zstr[i].Equals(mstr[j]) || mstr[j] == null)
                {
                    ++i;
                    ++j;
                }
                else
                {
                    j = next[j];
                }
            }
            if (j == mstr.Length)
                return i - mstr.Length;
            return -1;
        }

        static void GetNextVal<T>(T?[] str, int[] next) where T : struct
        {
            int i = 0;
            int j = -1;
            next[0] = -1;
            while (i < str.Length - 1)
            {
                if (j == -1 || str[i].Equals(str[j]) || str[i] == null)
                {
                    i++;
                    j++;
                    next[i] = j;
                }
                else
                {
                    j = next[j];
                }
            }
        }

        public static long? MinusAbs<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            long? equal = 0;

            foreach (var item in source)
            {
                long? val = selector(item);

                if (!val.HasValue)
                {
                    equal = null;
                    break;
                }

                equal = equal > val ? equal - val : val - equal;
            }

            return equal;
        }
    }
}