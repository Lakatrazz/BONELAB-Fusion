using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class SerializedGameObjectReference : IFusionSerializable {
        private enum ReferenceType {
            UNKNOWN = 0,
            FULL_PATH = 1,
            PROP_SYNCABLE = 2,
            RIG_MANAGER = 3,
            NULL = 4,
        }

        // Base gameobject reference
        public GameObject gameObject;

        public SerializedGameObjectReference() { }

        public SerializedGameObjectReference(GameObject go) {
            gameObject = go;
        }

        public void Serialize(FusionWriter writer) {
            if (gameObject == null) {
                writer.Write((byte)ReferenceType.NULL);
                return;
            }

            PhysicsRig physRig;

            // Check if there is a syncable, and write it
            if (PropSyncable.HostCache.TryGet(gameObject, out var syncable)) {
                writer.Write((byte)ReferenceType.PROP_SYNCABLE);
                writer.Write(syncable.GetId());
                writer.Write(syncable.GetIndex(gameObject).Value);
            }
            // Check if this is attached to a rigmanager
            else if ((physRig = gameObject.GetComponentInParent<PhysicsRig>()) != null) {
                if (PlayerRepUtilities.TryGetRigInfo(physRig.manager, out var smallId, out var references)) {
                    writer.Write((byte)ReferenceType.RIG_MANAGER);
                    writer.Write(smallId);

                    var rbIndex = references.GetIndex(gameObject.GetComponent<Rigidbody>()).Value;
                    writer.Write(rbIndex);

#if DEBUG
                    FusionLogger.Log($"Got rig info for a constraint with small id {smallId} and rb index {rbIndex}!");
#endif
                }
                else {
                    writer.Write((byte)ReferenceType.UNKNOWN);

#if DEBUG
                    FusionLogger.Warn("Failed to get rig info for a constraint!");
#endif
                }
            }
            // Write the full path to the object
            else {
                writer.Write((byte)ReferenceType.FULL_PATH);
                writer.Write(gameObject);
            }
        }
        
        public void Deserialize(FusionReader reader) {
            var type = (ReferenceType)reader.ReadByte();

            switch (type) {
                default:
                case ReferenceType.UNKNOWN:
                    // This should never happen
                    break;
                case ReferenceType.NULL:
                    // Do nothing for null
                    break;
                case ReferenceType.PROP_SYNCABLE:
                    var id = reader.ReadUInt16();
                    var index = reader.ReadUInt16();

                    if (SyncManager.TryGetSyncable(id, out var syncable) && syncable is PropSyncable prop) {
                        gameObject = prop.GetHost(index);
                    }
                    break;
                case ReferenceType.RIG_MANAGER:
                    var smallId = reader.ReadByte();
                    var rbIndex = reader.ReadByte();

                    if (PlayerRepUtilities.TryGetReferences(smallId, out var references)) {
                        var rb = references.GetRigidbody(rbIndex);
                        gameObject = rb.gameObject;

#if DEBUG
                        FusionLogger.Log($"Got constrained rig with small id {smallId} and rb index {rbIndex}!");
#endif
                    }
                    else {
#if DEBUG
                        FusionLogger.Warn($"Failed constraining Rig with small id {smallId} and rb index {rbIndex}!");
#endif
                    }
                    break;
                case ReferenceType.FULL_PATH:
                    gameObject = reader.ReadGameObject();
                    break;
            }
        }
    }
}
