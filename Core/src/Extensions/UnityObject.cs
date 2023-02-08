using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions
{
    public static class UnityObjectExtensions
    {
        /// <summary>
        /// Returns true if this object was garbage collected or is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static bool IsNOC(this UnityEngine.Object obj) {
            try {
                return obj is null || obj.WasCollected || obj == null;
            }
            catch { }

            return true;
        }
    }
}
