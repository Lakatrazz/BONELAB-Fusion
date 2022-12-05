using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ;
using SLZ.Interaction;
using LabFusion.Patching;
using LabFusion.Syncables;

using UnityEngine;

namespace LabFusion.Network
{
    public class PropSyncableCreateData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public string fullPath;
        public ushort id;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(fullPath);
            writer.Write(id);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            fullPath = reader.ReadString();
            id = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PropSyncableCreateData Create(byte smallId, string fullPath, ushort id)
        {
            return new PropSyncableCreateData()
            {
                smallId = smallId,
                fullPath = fullPath,
                id = id
            };
        }
    }

    [Net.DelayWhileLoading]
    public class PropSyncableCreateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PropSyncableCreate;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PropSyncableCreateData>())
                {

                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else
                    {
                        GameObject go;

                        if (data.fullPath != "_" && (go = GameObjectUtilities.GetGameObject(data.fullPath)))
                        {
                            var host = InteractableHost.Cache.Get(go);
                            PropSyncable syncable = null;

                            if (host)
                                syncable = new PropSyncable(host);
                            else
                                syncable = new PropSyncable(null, go);

                            SyncManager.RegisterSyncable(syncable, data.id);
                        }
                    }
                }
            }
        }
    }
}
