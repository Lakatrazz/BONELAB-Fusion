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
        public const int Size = sizeof(float) * 3;

        public Vector3 gravity;

        public static WorldGravityMessageData Create(Vector3 gravity) {
            return new WorldGravityMessageData() {
                gravity = gravity,
            };
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(gravity);
        }

        public void Deserialize(FusionReader reader)
        {
            gravity = reader.ReadVector3();
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
                        Physics.gravity = data.gravity;
                        PhysicsUtilities.CanModifyGravity = false;
                    }
                }
            }
        }
    }
}
