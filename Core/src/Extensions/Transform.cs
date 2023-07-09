using LabFusion.Data;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions {
    public static class TransformExtensions {
        public static Quaternion TransformRotation(this Transform transform, Quaternion rotation) => transform.rotation * rotation;

        public static Quaternion InverseTransformRotation(this Transform transform, Quaternion rotation) => Quaternion.Inverse(transform.rotation) * rotation;

        /// <summary>
        /// Causes a transform to look at the player.
        /// </summary>
        /// <param name="transform"></param>
        public static void LookAtPlayer(this Transform transform) {
            if (RigData.HasPlayer) {
                var rm = RigData.RigReferences.RigManager;
                var head = rm.physicsRig.m_head;
                transform.rotation = Quaternion.LookRotation(Vector3.Normalize(transform.position - head.position), head.up);
            }
        }

        internal static string GetBasePath(this Transform transform) {
            if (transform.parent == null)
                return $"{GameObjectUtilities.PathSeparator}{transform.name}";
            return $"{transform.parent.GetBasePath()}{GameObjectUtilities.PathSeparator}{GetSiblingNameIndex(transform)}{GameObjectUtilities.PathSeparator}{transform.name}";
        }

        internal static List<Transform> FindSiblingsWithName(this Transform parent, string name)
        {
            List<Transform> buffer = new List<Transform>(parent.childCount);

            for (var i = 0; i < parent.childCount; i++) {
                buffer.Add(parent.GetChild(i));
            }

            buffer.RemoveAll((t) => t.name != name);

            return buffer;
        }

        internal static int GetSiblingNameIndex(this Transform transform)
        {
            var locals = FindSiblingsWithName(transform.parent, transform.name);
            return locals.FindIndex((t) => t == transform);
        }

        internal static Transform GetTransformByIndex(this Transform parent, int index, string name)
        {
            // Get matching siblings
            var matching = FindSiblingsWithName(parent, name);
            if (matching.Count <= 0)
                return null;

            // Check if we can actually grab a transform with the index
            if (matching.Count <= index)
                return matching[matching.Count - 1];
            else if (index < 0)
                return matching[matching.Count - 1];
            else
                return matching[index];
        }
    }
}
