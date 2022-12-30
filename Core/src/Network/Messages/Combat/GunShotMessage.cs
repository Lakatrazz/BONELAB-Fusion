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

namespace LabFusion.Network
{
    public class GunShotData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public ushort gunId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(gunId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            gunId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static GunShotData Create(byte smallId, ushort gunId)
        {
            return new GunShotData()
            {
                smallId = smallId,
                gunId = gunId,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class GunShotMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GunShot;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<GunShotData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else
                    {
                        if (SyncManager.TryGetSyncable(data.gunId, out var gun) && gun is PropSyncable gunSyncable && gunSyncable.TryGetExtender<GunExtender>(out var extender))
                        {
                            // Fire the gun, make sure it has ammo in its mag so it can fire properly
                            var comp = extender.Component;

                            comp.hasFiredOnce = false;
                            comp._hasFiredSinceLastBroadcast = false;
                            comp.isTriggerPulledOnAttach = false;

                            comp.CeaseFire();
                            comp.Charge();

                            if (comp._magState != null)
                                comp._magState.Refill();

                            comp.SlideGrabbedReleased();
                            comp.SlideOverrideReleased();

                            GunPatches.IgnorePatches = true;
                            comp.Fire();
                            GunPatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
