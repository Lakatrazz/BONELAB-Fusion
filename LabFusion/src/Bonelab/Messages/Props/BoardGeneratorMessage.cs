using LabFusion.Entities;
using LabFusion.Bonelab.Patching;
using LabFusion.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Extenders;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Marrow.Extenders;

using UnityEngine;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Messages;

public class BoardPointData : INetSerializable
{
    public Vector3 Point;
    public bool HasBody;
    public ushort EntityID;
    public ushort BodyIndex;

    public MarrowBody GetMarrowBody()
    {
        if (!HasBody)
        {
            return null;
        }

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(EntityID);

        if (entity == null)
        {
            return null;
        }

        var extender = entity.GetExtender<MarrowBodyExtender>();

        if (extender == null)
        {
            return null;
        }

        return extender.GetComponent(BodyIndex);
    }

    public void Serialize(INetSerializer serializer)
    {
        var encodedPosition = NetworkTransformManager.EncodePosition(Point);

        serializer.SerializeValue(ref encodedPosition);
        serializer.SerializeValue(ref HasBody);
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref BodyIndex);

        if (serializer.IsReader)
        {
            Point = NetworkTransformManager.DecodePosition(encodedPosition);
        }
    }
}

public class BoardGeneratorData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte OwnerID;
    public ushort BoardID;
    public ushort BoardGeneratorID;

    public BoardPointData FirstPoint;
    public BoardPointData EndPoint;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerID);
        serializer.SerializeValue(ref BoardID);
        serializer.SerializeValue(ref BoardGeneratorID);

        serializer.SerializeValue(ref FirstPoint);
        serializer.SerializeValue(ref EndPoint);
    }
}

[Net.DelayWhileTargetLoading]
public class BoardGeneratorMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BoardGeneratorData>();

        var board = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.BoardID);

        if (board == null)
        {
            return;
        }

        var marrowEntityExtender = board.GetExtender<IMarrowEntityExtender>();

        if (marrowEntityExtender == null)
        {
            return;
        }

        // Get the marrow entity and body of the board
        var marrowEntity = marrowEntityExtender.MarrowEntity;

        var destructible = ObjectDestructible.Cache.Get(marrowEntity.gameObject);
        var marrowBody = marrowEntity.Bodies[0];

        // Scale the board between the points
        var distance = (data.FirstPoint.Point - data.EndPoint.Point).magnitude;
        var scale = BoardSpawnerAsyncPatches.GetBoardScale(distance);
        marrowEntity.transform.localScale = scale;

        // Scale mass to new size
        marrowEntity.ScaleRatio = distance;
        marrowEntity.ResetMass();

        // Get the board generator
        var boardGun = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.BoardGeneratorID);

        if (boardGun == null)
        {
            return;
        }

        var extender = boardGun.GetExtender<BoardGeneratorExtender>();

        if (extender == null)
        {
            return;
        }

        // Play SFX on the board gun
        var boardGenerator = extender.Component;
        boardGenerator.PlaySFX(boardGenerator.endSFX, data.EndPoint.Point);

        // Attach the board's joints
        boardGenerator.SetJoint(data.FirstPoint.Point, data.FirstPoint.GetMarrowBody(), marrowBody);
        boardGenerator.SetJoint(data.EndPoint.Point, data.EndPoint.GetMarrowBody(), marrowBody);

        // Add destruction event
        destructible.OnDestruction += (Action<ObjectDestructible>)boardGenerator.OnBoardDestruction;
    }
}