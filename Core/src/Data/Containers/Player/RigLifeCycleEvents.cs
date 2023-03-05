using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using MelonLoader;

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
