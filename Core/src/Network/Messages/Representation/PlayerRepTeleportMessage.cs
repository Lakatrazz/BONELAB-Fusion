using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Rig;
using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public class PlayerRepTeleportData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(float) * 3;

        public byte teleportedUser;
        public Vector3 position;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(teleportedUser);
            writer.Write(position);
        }

        public void Deserialize(FusionReader reader)
        {
            teleportedUser = reader.ReadByte();
            position = reader.ReadVector3();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepTeleportData Create(byte teleportedUser, Vector3 position)
        {
            return new PlayerRepTeleportData
            {
                teleportedUser = teleportedUser,
                position = position,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PlayerRepTeleportMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepTeleport;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PlayerRepTeleportData>()) {
                    // Only teleport if we aren't the server
                    if (!isServerHandled && data.teleportedUser == PlayerIdManager.LocalSmallId)
                    {
                        // Teleport the player
                        FusionPlayer.Teleport(data.position, Vector3Extensions.forward);
                    }
                    else
                        throw new ExpectedClientException();
                }
            }
        }
    }
}
