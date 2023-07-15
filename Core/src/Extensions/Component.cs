using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions {
    public static class ComponentExtensions {
        /// <summary>
        /// Returns true if this GameObject has a RigManager in the hierarchy.
        /// </summary>
        /// <param name="go"></param>
        public static bool IsPartOfPlayer(this GameObject go) {
            if (go == null)
                return false;

            return go.GetComponentInParent<RigManager>() != null;
        }

        /// <summary>
        /// Returns true if this Component has a RigManager in the hierarchy.
        /// </summary>
        /// <param name="comp"></param>
        public static bool IsPartOfPlayer(this Component comp) {
            if (comp == null)
                return false;

            return comp.GetComponentInParent<RigManager>() != null;
        }
    }
}
