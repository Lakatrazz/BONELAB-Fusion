using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.MarrowIntegration;
using LabFusion.Patching;
using LabFusion.Syncables;

using UnityEngine;

namespace LabFusion.Network
{
    public class GameObjectActiveData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public bool value;

        // Static values
        public GameObject gameObject;

        // PropSyncable values
        public ushort? syncId;
        public byte? scriptIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(value);

            // Static values
            writer.Write(gameObject);

            // PropSyncable values
            writer.Write(syncId);
            writer.Write(scriptIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            value = reader.ReadBoolean();

            // Static values
            gameObject = reader.ReadGameObject();

            // PropSyncable values
            syncId = reader.ReadUInt16Nullable();
            scriptIndex = reader.ReadByteNullable();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static GameObjectActiveData Create(byte smallId, bool value, SyncGameObjectEnabled script)
        {
            var data = new GameObjectActiveData() {
                smallId = smallId,
                value = value,
            };

            if (script.PropSyncable != null) {
                var syncable = script.PropSyncable;

                data.syncId = syncable.GetId();

                if (syncable.TryGetExtender<SyncGameObjectEnabledExtender>(out var extender)) {
                    data.scriptIndex = extender.GetIndex(script);
                }
            }
            else {
                data.gameObject = script.gameObject;
            }

            return data;
        }
    }

    public class GameObjectActiveMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GameObjectActive;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<GameObjectActiveData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        // Check what gameobject to search for
                        if (data.gameObject != null) {
                            data.gameObject.SetActive(data.value);
                        }
                        else if (data.syncId.HasValue && data.scriptIndex.HasValue 
                            && SyncManager.TryGetSyncable(data.syncId.Value, out var syncable) && syncable is PropSyncable prop && prop.TryGetExtender<SyncGameObjectEnabledExtender>(out var extender)) {

                            var script = extender.GetComponent(data.scriptIndex.Value);

                            if (script != null) {
                                script.gameObject.SetActive(data.value);
                            }
                        }
                    }
                }
            }
        }
    }
}
