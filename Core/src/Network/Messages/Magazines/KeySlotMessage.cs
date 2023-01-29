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

using LabFusion.Extensions;

using UnityEngine;

namespace LabFusion.Network
{
    public enum KeySlotType {
        UNKNOWN = 0,
        INSERT_STATIC = 1,
        INSERT_PROP = 2,
    }

    public class KeySlotData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public KeySlotType type;
        public ushort keyId;
        public GameObject receiver;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
            writer.Write(keyId);
            writer.Write(receiver);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (KeySlotType)reader.ReadByte();
            keyId = reader.ReadUInt16();
            receiver = reader.ReadGameObject();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static KeySlotData Create(byte smallId, KeySlotType type, ushort keyId, GameObject receiver)
        {
            return new KeySlotData()
            {
                smallId = smallId,
                type = type,
                keyId = keyId,
                receiver = receiver,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class KeySlotMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.KeySlot;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<KeySlotData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {

                        if (SyncManager.TryGetSyncable(data.keyId, out var key) && key is PropSyncable keySyncable && keySyncable.TryGetExtender<KeyExtender>(out var keyExtender)) {
                            KeyRecieverPatches.IgnorePatches = true;
                            
                            switch (data.type) {
                                default:
                                case KeySlotType.UNKNOWN:
                                    break;
                                case KeySlotType.INSERT_STATIC:
                                    if (data.receiver != null) {
                                        var keyReceiver = data.receiver.GetComponent<KeyReciever>();

                                        if (keyReceiver != null) {
                                            var host = InteractableHost.Cache.Get(keyExtender.Component.gameObject);

                                            // Insert the key and detach grips
                                            host.TryDetach();

                                            keyReceiver.OnInteractableHostEnter(host);
                                        }
                                    }
                                    break;
                                // TODO: Implement
                                case KeySlotType.INSERT_PROP:
                                    break;
                            }

                            KeyRecieverPatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
