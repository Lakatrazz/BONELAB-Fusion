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

using SLZ.Props;
using SLZ.Bonelab;

namespace LabFusion.Network
{
    public class BoardCreateData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort) + sizeof(int) + sizeof(float) * 7;

        public byte smallId;

        public ushort boardGunId;

        public int idx;
        public float mass;

        public Vector3 firstPoint;
        public Vector3 endPoint;

        public SerializedGameObjectReference firstRb;
        public SerializedGameObjectReference endRb;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);

            writer.Write(boardGunId);

            writer.Write(idx);
            writer.Write(mass);

            writer.Write(firstPoint);
            writer.Write(endPoint);

            writer.Write(firstRb);
            writer.Write(endRb);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();

            boardGunId = reader.ReadUInt16();

            idx = reader.ReadInt32();
            mass = reader.ReadSingle();

            firstPoint = reader.ReadVector3();
            endPoint = reader.ReadVector3();

            firstRb = reader.ReadFusionSerializable<SerializedGameObjectReference>();
            endRb = reader.ReadFusionSerializable<SerializedGameObjectReference>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static BoardCreateData Create(byte smallId, ushort boardGunId, BoardGenerator generator, int idx, float mass)
        {
            var data = new BoardCreateData()
            {
                smallId = smallId,
                boardGunId = boardGunId,
                idx = idx,
                mass = mass,
                firstPoint = generator.firstPoint,
                endPoint = generator.EndPoint,
            };

            // Write first rigidbody
            if (generator.FirstRb)
                data.firstRb = new SerializedGameObjectReference(generator.FirstRb.gameObject);
            else
                data.firstRb = new SerializedGameObjectReference(null);

            // Write second rigidbody
            if (generator.EndRb)
                data.endRb = new SerializedGameObjectReference(generator.EndRb.gameObject);
            else
                data.endRb = new SerializedGameObjectReference(null);

            return data;
        }
    }

    [Net.DelayWhileTargetLoading]
    public class BoardCreateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BoardCreate;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<BoardCreateData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (SyncManager.TryGetSyncable(data.boardGunId, out var syncable) && syncable is PropSyncable boardGun && boardGun.TryGetExtender<BoardGeneratorExtender>(out var extender)) {
                            var comp = extender.Component;

                            // Assign points
                            comp.firstPoint = data.firstPoint;
                            comp.EndPoint = data.endPoint;

                            // Assign rigidbodies
                            if (data.firstRb.gameObject != null)
                                comp.FirstRb = data.firstRb.gameObject.GetComponent<Rigidbody>();

                            if (data.endRb.gameObject != null)
                                comp.EndRb = data.endRb.gameObject.GetComponent<Rigidbody>();

                            // Create board
                            BoardGeneratorPatches.IgnorePatches = true;

                            comp.BoardSpawner(data.idx, data.mass);

                            BoardGeneratorPatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
