using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class MetadataHelper {
        public const string UsernameKey = "Username";

        public const string LoadingKey = "IsLoading";

        public static bool ParseBool(string value) => value == "True";

        public static string ParseString(bool value) => value ? "True" : "False";
    }
}
