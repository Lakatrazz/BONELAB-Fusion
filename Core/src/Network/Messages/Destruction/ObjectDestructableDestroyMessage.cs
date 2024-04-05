using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;
using UnityEngine;
using SLZ.Marrow.Data;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    [Net.DelayWhileTargetLoading]
    public class ObjectDestructableDestroyMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ObjectDestructableDestroy;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            using var data = reader.ReadFusionSerializable<ComponentIndexData>();
            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                if (SyncManager.TryGetSyncable<PropSyncable>(data.syncId, out var destructable) && destructable.TryGetExtender<ObjectDestructableExtender>(out var extender))
                {
                    var objectDestructable = extender.GetComponent(data.componentIndex);
                    ObjectDestructablePatches.IgnorePatches = true;
                    PooleeDespawnPatch.IgnorePatch = true;

                    objectDestructable._hits = objectDestructable.reqHitCount + 1;
                    objectDestructable.TakeDamage(Vector3Extensions.up, objectDestructable._health + 1f, false, AttackType.Blunt);

                    ObjectDestructablePatches.IgnorePatches = false;
                    PooleeDespawnPatch.IgnorePatch = false;
                }
            }
        }
    }
}
