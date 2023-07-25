using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class StringExtensions {
        public const char UniqueSeparator = '¬';

        public static string Contract(this IList<string> list) {
            StringBuilder builder = new();
            for (var i = 0; i < list.Count; i++) {
                var value = list[i];
                builder.Append(value);

                if (i + 1 < list.Count)
                    builder.Append(UniqueSeparator);
            }

            return builder.ToString();
        }

        public static string[] Expand(this string value) {
            if (string.IsNullOrEmpty(value))
                return new string[0];

            return value.Split(UniqueSeparator);
        }

        public static string RemoveRichText(this string str) {
            Regex rich = new(@"<[^>]*>");
            string plainText = str;

            if (rich.IsMatch(plainText)) {
                plainText = rich.Replace(plainText, string.Empty);
            }

            return plainText;
        }

        public static string LimitLength(this string str, int maxLength) {
            if (string.IsNullOrEmpty(str))
                return str;

            int plainLength = RemoveRichText(str).Length;

            if (plainLength <= maxLength) {
                return str;
            }

            int offset = str.Length - plainLength;

            return str.Substring(0, maxLength + offset);
        }

        public static int GetSize(this string str) {
            return GetSize(str, Encoding.UTF8);
        }

        public static int GetSize(this string str, Encoding encoding) {
            return encoding.GetByteCount(str) + 4;
        }
    }
}
