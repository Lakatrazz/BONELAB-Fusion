using LabFusion.Data;
using LabFusion.Utilities;

using UnityEngine;



namespace LabFusion.Network
{
    public class WorldGravityMessageData : IFusionSerializable
    {
        public const int Size = sizeof(float) * 3;

        public Vector3 gravity;

        public static WorldGravityMessageData Create(Vector3 gravity)
        {
            return new WorldGravityMessageData()
            {
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
    }

    public class WorldGravityMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.WorldGravity;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!NetworkInfo.IsServer)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<WorldGravityMessageData>();

                PhysicsUtilities.CanModifyGravity = true;
                Physics.gravity = data.gravity;
                PhysicsUtilities.CanModifyGravity = false;
            }
        }
    }
}
