using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Network;

public class PlayerRepTeleportData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(float) * 3;

    public byte teleportedUser;
    public Vector3 position;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(teleportedUser);
        writer.Write(NetworkTransformManager.EncodePosition(position));
    }

    public void Deserialize(FusionReader reader)
    {
        teleportedUser = reader.ReadByte();
        position = NetworkTransformManager.DecodePosition(reader.ReadVector3());
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
public class PlayerRepTeleportMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepTeleport;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Don't teleport if we're the server
        if (isServerHandled)
        {
            throw new ExpectedClientException();
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerRepTeleportData>();

        // Make sure this is us
        if (data.teleportedUser != PlayerIdManager.LocalSmallId)
        {
            return;
        }

        // Teleport the player
        FusionPlayer.Teleport(data.position, Vector3Extensions.forward);
    }
}