using LabFusion.Syncables;
using LabFusion.Patching;
using Il2CppSLZ.Marrow.Data;
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
            var data = reader.ReadFusionSerializable<ComponentIndexData>();
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
