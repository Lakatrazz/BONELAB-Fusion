using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Patching;

using UnityEngine;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class BoardPointData : IFusionSerializable
{
    public Vector3 point;
    public bool hasBody;
    public ushort entityId;
    public ushort bodyIndex;

    public MarrowBody GetMarrowBody()
    {
        if (!hasBody)
        {
            return null;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(entityId);

        if (entity == null)
        {
            return null;
        }

        var extender = entity.GetExtender<MarrowBodyExtender>();

        if (extender == null)
        {
            return null;
        }

        return extender.GetComponent(bodyIndex);
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(point);
        writer.Write(hasBody);
        writer.Write(entityId);
        writer.Write(bodyIndex);
    }

    public void Deserialize(FusionReader reader)
    {
        point = reader.ReadVector3();
        hasBody = reader.ReadBoolean();
        entityId = reader.ReadUInt16();
        bodyIndex = reader.ReadUInt16();
    }

    public static BoardPointData Create(Vector3 point, bool hasBody, ushort entityId, ushort bodyIndex)
    {
        return new BoardPointData()
        {
            point = point,
            hasBody = hasBody,
            entityId = entityId,
            bodyIndex = bodyIndex,
        };
    }
}

public class BoardGeneratorData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte ownerId;
    public ushort boardId;
    public ushort boardGeneratorId;

    public BoardPointData firstPoint;
    public BoardPointData endPoint;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(ownerId);
        writer.Write(boardId);
        writer.Write(boardGeneratorId);

        writer.Write(firstPoint);
        writer.Write(endPoint);
    }

    public void Deserialize(FusionReader reader)
    {
        ownerId = reader.ReadByte();
        boardId = reader.ReadUInt16();
        boardGeneratorId = reader.ReadUInt16();

        firstPoint = reader.ReadFusionSerializable<BoardPointData>();
        endPoint = reader.ReadFusionSerializable<BoardPointData>();
    }

    public static BoardGeneratorData Create(byte ownerId, ushort boardId, ushort boardGeneratorId, BoardPointData firstPoint, BoardPointData endPoint)
    {
        return new BoardGeneratorData()
        {
            ownerId = ownerId,
            boardId = boardId,
            boardGeneratorId = boardGeneratorId,

            firstPoint = firstPoint,
            endPoint = endPoint,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class BoardGeneratorMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.BoardGenerator;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Send message to all clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<BoardGeneratorData>();

        var board = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.boardId);

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
        var distance = (data.firstPoint.point - data.endPoint.point).magnitude;
        var scale = BoardSpawnerAsyncPatches.GetBoardScale(distance);
        marrowEntity.transform.localScale = scale;

        // Scale mass to new size
        marrowEntity.ScaleRatio = distance;
        marrowEntity.ResetMass();

        // Get the board generator
        var boardGun = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.boardGeneratorId);

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
        boardGenerator.PlaySFX(boardGenerator.endSFX, data.endPoint.point);

        // Attach the board's joints
        boardGenerator.SetJoint(data.firstPoint.point, data.firstPoint.GetMarrowBody(), marrowBody);
        boardGenerator.SetJoint(data.endPoint.point, data.endPoint.GetMarrowBody(), marrowBody);

        // Add destruction event
        destructible.OnDestruction += (Action<ObjectDestructible>)boardGenerator.OnBoardDestruction;
    }
}