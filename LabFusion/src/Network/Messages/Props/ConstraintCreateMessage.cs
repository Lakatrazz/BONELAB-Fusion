using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.SDK.Achievements;
using LabFusion.Entities;
using LabFusion.Scene;

using UnityEngine;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ConstraintCreateData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort) * 3 + sizeof(float) * 12;

    public byte smallId;

    public ushort constrainerId;

    public Constrainer.ConstraintMode mode;

    public SerializedGameObjectReference tracker1;
    public SerializedGameObjectReference tracker2;

    public SerializedTransform tracker1Transform;
    public SerializedTransform tracker2Transform;

    public Vector3 point1;
    public Vector3 point2;

    public Vector3 normal1;
    public Vector3 normal2;

    public ushort point1Id;
    public ushort point2Id;

    public void Serialize(INetSerializer serializer)
    {
        var encodedPoint1 = NetworkTransformManager.EncodePosition(point1);
        var encodedPoint2 = NetworkTransformManager.EncodePosition(point2);

        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref constrainerId);
        serializer.SerializeValue(ref mode, Precision.OneByte);

        serializer.SerializeValue(ref tracker1);
        serializer.SerializeValue(ref tracker2);

        serializer.SerializeValue(ref tracker1Transform);
        serializer.SerializeValue(ref tracker2Transform);

        serializer.SerializeValue(ref encodedPoint1);
        serializer.SerializeValue(ref encodedPoint2);

        serializer.SerializeValue(ref normal1);
        serializer.SerializeValue(ref normal2);

        serializer.SerializeValue(ref point1Id);
        serializer.SerializeValue(ref point2Id);

        if (serializer.IsReader)
        {
            point1 = NetworkTransformManager.DecodePosition(encodedPoint1);
            point2 = NetworkTransformManager.DecodePosition(encodedPoint2);
        }
    }

    public static ConstraintCreateData Create(byte smallId, ushort constrainerId, ConstrainerPointPair pair)
    {
        return new ConstraintCreateData()
        {
            smallId = smallId,
            constrainerId = constrainerId,
            mode = pair.mode,
            tracker1 = new SerializedGameObjectReference(pair.go1),
            tracker2 = new SerializedGameObjectReference(pair.go2),
            tracker1Transform = new SerializedTransform(pair.go1.transform),
            tracker2Transform = new SerializedTransform(pair.go2.transform),
            point1 = pair.point1,
            point2 = pair.point2,
            normal1 = pair.normal1,
            normal2 = pair.normal2,

            // These are unknown by the client, but are set by the server
            point1Id = 0,
            point2Id = 0,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ConstraintCreateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConstraintCreate;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstraintCreateData>();

        var constrainerEntity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.constrainerId);
        bool hasConstrainer = constrainerEntity != null;

        // Send message to other clients if server
        if (received.IsServerHandled)
        {
            // Make sure we have a constrainer server side (and it's being held)
            if (hasConstrainer)
            {
                // Recreate the message so we can assign server-side sync ids
                data.point1Id = NetworkEntityManager.IdManager.RegisteredEntities.AllocateNewId();
                data.point2Id = NetworkEntityManager.IdManager.RegisteredEntities.AllocateNewId();

                MessageRelay.RelayNative(data, Tag, NetworkChannel.Reliable, RelayType.ToClients);
            }

            return;
        }

        if (!data.tracker1.gameObject || !data.tracker2.gameObject)
        {
            return;
        }

        // Check if player constraining is disabled and if this is attempting to constrain a player
        if (!ConstrainerUtilities.PlayerConstraintsEnabled)
        {
            if (data.tracker1.gameObject.IsPartOfPlayer() || data.tracker2.gameObject.IsPartOfPlayer())
                return;
        }

        if (!ConstrainerUtilities.HasConstrainer)
        {
            return;
        }

        // Get the synced constrainer
        // This isn't required for client constraint creation, but is used for SFX and VFX
        Constrainer syncedComp = null;

        if (hasConstrainer)
        {
            var extender = constrainerEntity.GetExtender<ConstrainerExtender>();

            syncedComp = extender?.Component;
        }

        hasConstrainer = syncedComp != null;

        var comp = ConstrainerUtilities.GlobalConstrainer;
        comp.mode = data.mode;

        // Setup points
        comp._point1 = data.point1;
        comp._point2 = data.point2;

        comp._normal1 = data.normal1;
        comp._normal2 = data.normal2;

        // Setup gameobjects
        comp._gO1 = data.tracker1.gameObject;
        comp._gO2 = data.tracker2.gameObject;
        comp._mb1 = comp._gO1.GetComponentInChildren<MarrowBody>(true);
        comp._mb2 = comp._gO2.GetComponentInChildren<MarrowBody>(true);

        // Store positions
        Transform tran1 = comp._gO1.transform;
        Transform tran2 = comp._gO2.transform;

        Vector3 go1Pos = tran1.position;
        Quaternion go1Rot = tran1.rotation;

        Vector3 go2Pos = tran2.position;
        Quaternion go2Rot = tran2.rotation;

        // Force positions
        tran1.SetPositionAndRotation(data.tracker1Transform.position, data.tracker1Transform.rotation);
        tran2.SetPositionAndRotation(data.tracker2Transform.position, data.tracker2Transform.rotation);

        // Create the constraint
        ConstrainerPatches.IsReceivingConstraints = true;
        ConstrainerPatches.FirstId = data.point1Id;
        ConstrainerPatches.SecondId = data.point2Id;

        if (hasConstrainer)
            comp.LineMaterial = syncedComp.LineMaterial;

        comp.PrimaryButtonUp();

        ConstrainerPatches.FirstId = 0;
        ConstrainerPatches.SecondId = 0;
        ConstrainerPatches.IsReceivingConstraints = false;

        // Reset positions
        tran1.SetPositionAndRotation(go1Pos, go1Rot);
        tran2.SetPositionAndRotation(go2Pos, go2Rot);

        // Events when the constrainer is from another player
        if (data.smallId != PlayerIdManager.LocalSmallId)
        {
            if (hasConstrainer)
            {
                // Play sound
                syncedComp.sfx.GravLocked();
                syncedComp.sfx.Release();
            }

            // Check for host constraint achievement
            if (data.smallId == PlayerIdManager.HostSmallId && AchievementManager.TryGetAchievement<ClassStruggle>(out var achievement))
            {
                if (!achievement.IsComplete && (tran1.IsPartOfSelf() || tran2.IsPartOfSelf()))
                {
                    achievement.IncrementTask();
                }
            }
        }
    }
}