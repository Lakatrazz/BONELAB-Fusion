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
        public string gameObjectPath;
        public ushort id;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(gameObjectPath);
            writer.Write(id);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            gameObjectPath = reader.ReadString();
            id = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PropSyncableCreateData Create(byte smallId, string gameObjectPath, ushort id)
        {
            return new PropSyncableCreateData()
            {
                smallId = smallId,
                gameObjectPath = gameObjectPath,
                id = id
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PropSyncableCreateMessage : FusionMessageHandlerAsync
    {
        public override byte? Tag => NativeMessageTag.PropSyncableCreate;

        public override async Task HandleMessageAsync(byte[] bytes, bool isServerHandled = false)
        {
            await Task.Delay(16);
            ThreadingUtilities.IL2PrepareThread();

            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PropSyncableCreateData>())
                {
                    GameObject gameObject = await GameObjectUtilities.GetGameObjectAsync(data.gameObjectPath);

                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        bool isCompleted = false;
                        ThreadingUtilities.RunSynchronously(() => {
                            // If the object is blacklisted, don't bother sending the message to others
                            var go = gameObject;

                            if (go != null && !go.IsSyncWhitelisted()) {
                                isCompleted = true;
                                return;
                            }

                            using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                            }

                            isCompleted = true;
                        });

                        while (!isCompleted)
                            await Task.Delay(16);
                    }
                    else
                    {
                        bool isCompleted = false;
                        ThreadingUtilities.RunSynchronously(() =>
                        {
                            if (gameObject != null)
                            {
                                // Check if its blacklisted
                                if (!gameObject.IsSyncWhitelisted()) {
                                    isCompleted = true;
                                    return;
                                }

                                var host = InteractableHost.Cache.Get(gameObject);
                                PropSyncable syncable;

                                if (host)
                                    syncable = new PropSyncable(host);
                                else
                                    syncable = new PropSyncable(null, gameObject);

                                SyncManager.RegisterSyncable(syncable, data.id);

                                syncable.SetOwner(data.smallId);

                                // Insert catchup hook for future users
                                if (NetworkInfo.IsServer)
                                    syncable.InsertCatchupDelegate((id) => {
                                        PropSender.SendCatchupCreation(syncable, id);
                                    });

                                isCompleted = true;
                            }
                        });

                        while (!isCompleted)
                            await Task.Delay(16);
                    }
                }
            }
        }
    }
}
