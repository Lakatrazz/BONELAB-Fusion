using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Network;
using LabFusion.Senders;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Auto Sync On Start")]
    [DisallowMultipleComponent]
#endif
    public sealed class AutoSyncOnStart : FusionMarrowBehaviour {
#if MELONLOADER
        public AutoSyncOnStart(IntPtr intPtr) : base(intPtr) { }

        public void Start()
        {
            if (NetworkInfo.IsServer)
                PropSender.SendPropCreation(gameObject);
        }
#else
        public override string Comment => "This script will automatically sync this object to the host, so that clients see it in the same position and rotation.\n" +
            "Please note that there needs to be at least one rigidbody on the object, and it is recommended that the rigidbody moves with physics instead of kinematics!";

        public void Start() 
        {
        }
#endif
    }
}
