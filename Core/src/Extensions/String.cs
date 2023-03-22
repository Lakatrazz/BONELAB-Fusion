using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class StringExtensions {
        public static int GetSize(this string str) {
            return GetSize(str, Encoding.UTF8);
        }

        public static int GetSize(this string str, Encoding encoding) {
            return encoding.GetByteCount(str) + 4;
        }
    }
}
