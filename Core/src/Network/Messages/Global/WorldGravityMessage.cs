using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public class WorldGravityMessageData : IFusionSerializable, IDisposable
    {
        public ulong gravity;

        public static WorldGravityMessageData Create(Vector3 gravity) {
            return new WorldGravityMessageData() {
                gravity = gravity.ToULong(true)
            };
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(gravity);
        }

        public void Deserialize(FusionReader reader)
        {
            gravity = reader.ReadUInt64();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }

    [Net.SkipHandleWhileLoading]
    public class WorldGravityMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.WorldGravity;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!NetworkInfo.CurrentNetworkLayer.IsServer) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<WorldGravityMessageData>()) {
                        PhysicsUtilities.CanModifyGravity = true;
                        Physics.gravity = data.gravity.ToVector3();
                        PhysicsUtilities.CanModifyGravity = false;
                    }
                }
            }
        }
    }
}
