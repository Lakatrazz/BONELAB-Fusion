using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using static MelonLoader.MelonLogger;

namespace LabFusion.Extensions {
    public static class JointExtensions {
        internal static bool IgnoreDriveCheck = false;

        public enum JointDriveAxis {
            XDrive,
            YDrive,
            ZDrive,
            AngularXDrive,
            AngularYZDrive,
            SlerpDrive
        }

        internal static void CheckSyncableDrive(this ConfigurableJoint joint, JointDriveAxis axis, ref JointDrive drive) {
            if (IgnoreDriveCheck)
                return;

            try {
                if (NetworkInfo.HasServer && PropSyncable.JointCache.TryGetValue(joint, out var syncable))
                {
                    syncable.OnSetDrive(joint, drive, axis);

                    if (!syncable.CanSetJointDrives)
                        drive = new JointDrive();
                }
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("checking for joint drive", e);
#endif
            }
        }

        public static Vector3 GetLocalAnchor(this Joint joint, Vector3 anchor) {
            return joint.transform.InverseTransformPoint(anchor);
        }

        public static Vector3 GetLocalConnectedAnchor(this Joint joint, Vector3 anchor) {
            return joint.connectedBody ?  joint.connectedBody.transform.InverseTransformPoint(anchor) : anchor;
        }

        public static Vector3 GetWorldConnectedAnchor(this Joint joint, Vector3 anchor) {
            return joint.connectedBody ? joint.connectedBody.transform.TransformPoint(anchor) : anchor;
        }
    }
}
