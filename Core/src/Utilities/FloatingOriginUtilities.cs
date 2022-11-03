using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    /// <summary>
    /// Utility class for managing all floating origin logic.
    /// </summary>
    public static class FloatingOriginUtilities {
        /// <summary>
        /// The origin of the world.
        /// </summary>
        public static Vector3 Origin { get; private set; } = Vector3.zero;

        /// <summary>
        /// Sets the world floating origin.
        /// </summary>
        /// <param name="origin"></param>
        public static void SetFloatingOrigin(Vector3 origin) {
            Origin = origin;
        }

        /// <summary>
        /// Converts the world space position to local space relative to the floating origin.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 ConvertToLocalSpace(Vector3 position) {
            return position - Origin;
        }

        /// <summary>
        /// Converts the origin local position to a world space position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 ConvertToWorldSpace(Vector3 position) {
            return position + Origin;
        }
    }
}
