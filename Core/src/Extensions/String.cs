using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class StringExtensions {
        public static string LimitLength(this string str, int maxLength) {
            if (string.IsNullOrEmpty(str))
                return str;

            // Get a version of the string without rich text tags 
            Regex rich = new Regex(@"<[^>]*>");
            string plainText = str;

            if (rich.IsMatch(plainText)) {
                plainText = rich.Replace(plainText, string.Empty);
            }

            int plainLength = plainText.Length;

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
