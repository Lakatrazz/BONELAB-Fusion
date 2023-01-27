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
    public enum DescentNooseType {
        UNKNOWN = 0,
        ATTACH_NOOSE = 1,
        CUT_NOOSE = 2,
    }

    public class DescentNooseData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public DescentNooseType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (DescentNooseType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static DescentNooseData Create(byte smallId, DescentNooseType type)
        {
            return new DescentNooseData()
            {
                smallId = smallId,
                type = type,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class DescentNooseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DescentNoose;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<DescentNooseData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        NoosePatches.IgnorePatches = true;

                        switch (data.type) {
                            default:
                            case DescentNooseType.UNKNOWN:
                                break;
                            case DescentNooseType.ATTACH_NOOSE:
                                if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                                    DescentData.Noose.rM = rep.RigReferences.RigManager;
                                    DescentData.Noose.AttachNeck();
                                }
                                break;
                            case DescentNooseType.CUT_NOOSE:
                                DescentData.Noose.NooseCut();
                                break;
                        }

                        NoosePatches.IgnorePatches = false;
                    }
                }
            }
        }
    }
}
