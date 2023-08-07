using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FusionHelper.Extensions {
    public static class StringExtensions {
        public const char UniqueSeparator = '¬';

        public static string[] Expand(this string value) {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<string>();

            return value.Split(UniqueSeparator);
        }
    }
}
