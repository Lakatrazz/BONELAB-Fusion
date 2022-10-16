using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions {
    public static class TransformExtensions {
        public static string GetPath(this Transform transform) {
            if (transform.parent == null)
                return "/" + transform.name;
            return transform.parent.GetPath() + "/" + transform.name;
        }
    }
}
