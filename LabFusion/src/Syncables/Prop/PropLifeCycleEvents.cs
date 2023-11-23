﻿using System;
using MelonLoader;
using UnityEngine;

namespace LabFusion.Syncables
{
    [RegisterTypeInIl2Cpp]
    public sealed class PropLifeCycleEvents : MonoBehaviour
    {
        public PropLifeCycleEvents(IntPtr intPtr) : base(intPtr) { }

        public PropSyncable Syncable;

        private void OnEnable()
        {
            // Make sure our syncable exists
            if (Syncable == null)
                return;

            Syncable.IsRootEnabled = true;
        }

        private void OnDisable()
        {
            // Make sure our syncable exists
            if (Syncable == null)
                return;

            Syncable.IsRootEnabled = false;
        }

        private void OnDestroy()
        {
            // Make sure our syncable exists
            if (Syncable == null)
                return;

            // If the GameObject is disabled or destroyed, we might as well just remove the syncable
            SyncManager.RemoveSyncable(Syncable);
        }
    }
}
