using System;
using MelonLoader;
using UnityEngine;

namespace LabFusion.Data
{
    [RegisterTypeInIl2Cpp]
    public sealed class RigLifeCycleEvents : MonoBehaviour
    {
        public RigLifeCycleEvents(IntPtr intPtr) : base(intPtr) { }

        public RigReferenceCollection Collection;

        private void OnDestroy()
        {
            // Make sure our collection exists
            if (Collection == null)
                return;

            Collection.OnDestroy();
        }
    }
}
