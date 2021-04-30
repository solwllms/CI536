using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CI536
{
    public static class Util
    {
        public static string ToSafeString(this string value)
        {
            return value.Replace("®", "").Replace("™", "");
        }

        // by Joan Caron
        // https://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c
        public static string ToSlug(this string value)
        {
            //First to lower case
            value = value.ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
            value = Encoding.ASCII.GetString(bytes);

            //Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", "", RegexOptions.Compiled);

            //Trim dashes from end
            value = value.Trim('-', '_');

            //Replace double occurences of - or _
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }
        // will split a given array into chunks of a given maximum size
        public static List<List<T>> Split<T>(this List<T> array, int size)
        {
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < (int)Math.Ceiling((float)array.Count() / size); i++)
            {
                int remaining = ((i * size) + size) > array.Count() ? array.Count() - (i * size) : size;
                list.Add(array.GetRange(i * size, remaining));
            }

            return list;
        }
    }
}
