using LabFusion.Utilities;
using Il2CppSLZ.Rig;

using UnityEngine;

namespace LabFusion.Extensions
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Returns true if this GameObject has a RigManager in the hierarchy.
        /// </summary>
        /// <param name="go"></param>
        public static bool IsPartOfPlayer(this GameObject go)
        {
            if (go == null)
                return false;

            return go.GetComponentInParent<RigManager>() != null;
        }

        /// <summary>
        /// Returns true if this Component has a RigManager in the hierarchy.
        /// </summary>
        /// <param name="comp"></param>
        public static bool IsPartOfPlayer(this Component comp)
        {
            if (comp == null)
                return false;

            return comp.GetComponentInParent<RigManager>() != null;
        }

        /// <summary>
        /// Returns true if this GameObject is part of the local player.
        /// </summary>
        /// <param name="go"></param>
        public static bool IsPartOfSelf(this GameObject go)
        {
            if (go == null)
                return false;

            return go.GetComponentInParent<RigManager>().IsSelf();
        }

        /// <summary>
        /// Returns true if this Component is part of the local player.
        /// </summary>
        /// <param name="comp"></param>
        public static bool IsPartOfSelf(this Component comp)
        {
            if (comp == null)
                return false;

            return comp.GetComponentInParent<RigManager>().IsSelf();
        }
    }
}
