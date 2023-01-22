using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.AI;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class SerializedTriggerRefReference : IFusionSerializable {
        private enum ReferenceType {
            UNKNOWN = 0,
            PROP_SYNCABLE = 1,
            RIG_MANAGER = 2,
            NULL = 3,
        }

        // Base trigger ref reference
        public TriggerRefProxy proxy;

        public SerializedTriggerRefReference() { }

        public SerializedTriggerRefReference(TriggerRefProxy proxy) {
            this.proxy = proxy;
        }

        public void Serialize(FusionWriter writer) {
            if (proxy == null) {
                writer.Write((byte)ReferenceType.NULL);
                return;
            }

            PhysicsRig physRig;

            // Check if there is a syncable, and write it
            if (TriggerRefProxyExtender.Cache.TryGet(proxy, out var syncable)) {
                writer.Write((byte)ReferenceType.PROP_SYNCABLE);
                writer.Write(syncable.GetId());
            }
            // Check if this is attached to a rigmanager
            else if ((physRig = proxy.GetComponentInParent<PhysicsRig>()) != null) {
                if (PlayerRepUtilities.TryGetRigInfo(physRig.manager, out var smallId, out var references)) {
                    writer.Write((byte)ReferenceType.RIG_MANAGER);
                    writer.Write(smallId);
                }
                else {
                    writer.Write((byte)ReferenceType.UNKNOWN);

#if DEBUG
                    FusionLogger.Warn("Failed to get rig info for a trigger ref!");
#endif
                }
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

                    if (SyncManager.TryGetSyncable(id, out var syncable) && syncable is PropSyncable prop && prop.TryGetExtender<TriggerRefProxyExtender>(out var extender)) {
                        proxy = extender.Component;
                    }
                    break;
                case ReferenceType.RIG_MANAGER:
                    var smallId = reader.ReadByte();

                    if (PlayerRepUtilities.TryGetReferences(smallId, out var references)) {
                        proxy = references.Proxy;
                    }
                    break;
            }
        }
    }
}
