using LabFusion.Data;
using LabFusion.Utilities;
using Il2CppSLZ.Interaction;
using LabFusion.Syncables;

using UnityEngine;
using LabFusion.Senders;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class PropSyncableCreateData : IFusionSerializable
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

            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PropSyncableCreateData>();
            GameObject gameObject = await GameObjectUtilities.GetGameObjectAsync(data.gameObjectPath);

            // Send message to other clients if server
            if (isServerHandled)
            {
                bool isCompleted = false;
                ThreadingUtilities.RunSynchronously(() =>
                {
                    // If the object is blacklisted, don't bother sending the message to others
                    var go = gameObject;

                    if (go != null && !go.IsSyncWhitelisted())
                    {
                        isCompleted = true;
                        return;
                    }

                    using (var message = FusionMessage.Create(Tag.Value, bytes))
                    {
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
                        if (!gameObject.IsSyncWhitelisted())
                        {
                            isCompleted = true;
                            return;
                        }

                        var marrowEntity = gameObject.GetComponentInParent<MarrowEntity>(true);
                        NetworkEntity networkEntity = new();
                        NetworkProp networkProp = new(networkEntity, marrowEntity);

                        NetworkEntityManager.IdManager.RegisterEntity(data.id, networkEntity);

                        networkEntity.SetOwner(PlayerIdManager.GetPlayerId(data.id));

                        // Insert catchup hook for future users
                        networkEntity.OnEntityCatchup += ((entity, player) =>
                        {
                            PropSender.SendCatchupCreation(networkProp, player);
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
