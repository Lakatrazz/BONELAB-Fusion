using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Object = UnityEngine.Object;

namespace LabFusion.Extensions
{
    public static class UnityObjectExtensions
    {
        /// <summary>
        /// Returns true if both objects are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lft"></param>
        /// <param name="rht"></param>
        /// <returns></returns>
        public static bool EqualsIL2CPP<T>(this T lft, T rht) {
            // Compare instance ids for unity objects
            if (lft is Object objLft && rht is Object objRht) {
                return objLft.GetInstanceID() == objRht.GetInstanceID();
            }

            return lft.GetHashCode() == rht.GetHashCode();
        }

        /// <summary>
        /// Returns true if this object was garbage collected or is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static bool IsNOC(this Object obj) {
            try {
                return obj is null || obj.WasCollected || obj == null;
            }
            catch { }

            return true;
        }
    }
}
