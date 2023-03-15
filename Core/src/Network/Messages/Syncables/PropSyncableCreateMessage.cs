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
using LabFusion.Senders;

namespace LabFusion.Network
{
    public class PropSyncableCreateData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte smallId;
        public GameObject gameObject;
        public ushort id;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(gameObject);
            writer.Write(id);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            gameObject = reader.ReadGameObject();
            id = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PropSyncableCreateData Create(byte smallId, GameObject gameObject, ushort id)
        {
            return new PropSyncableCreateData()
            {
                smallId = smallId,
                gameObject = gameObject,
                id = id
            };
        }
    }

    [Net.DelayWhileTargetLoading]
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
                        // If the object is blacklisted, don't bother sending the message to others
                        var go = data.gameObject;

                        if (go != null && !go.IsSyncWhitelisted()) {
                            return;
                        }

                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else
                    {
                        if (data.gameObject != null)
                        {
                            var go = data.gameObject;

                            // Check if its blacklisted
                            if (!go.IsSyncWhitelisted())
                                return;

                            var host = InteractableHost.Cache.Get(go);
                            PropSyncable syncable;

                            if (host)
                                syncable = new PropSyncable(host);
                            else
                                syncable = new PropSyncable(null, go);

                            SyncManager.RegisterSyncable(syncable, data.id);

                            syncable.SetOwner(data.smallId);

                            // Insert catchup hook for future users
                            if (NetworkInfo.IsServer)
                                syncable.InsertCatchupDelegate((id) => {
                                    PropSender.SendCatchupCreation(syncable, id);
                                });
                        }
                    }
                }
            }
        }
    }
}
