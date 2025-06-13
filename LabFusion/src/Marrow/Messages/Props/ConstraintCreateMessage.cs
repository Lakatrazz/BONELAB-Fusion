using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Marrow.Patching;
using LabFusion.SDK.Achievements;
using LabFusion.Entities;
using LabFusion.Scene;
using LabFusion.Network.Serialization;
using LabFusion.Data.Serializables;
using LabFusion.SDK.Modules;
using LabFusion.Safety;

using UnityEngine;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Messages;

public class ConstraintCreateData : INetSerializable
{
    public byte SmallID;

    public ushort? ConstrainerID;

    public Constrainer.ConstraintMode Mode;

    public GameObjectReference Tracker1;
    public GameObjectReference Tracker2;

    public SerializedTransform Tracker1Transform;
    public SerializedTransform Tracker2Transform;

    public Vector3 Point1;
    public Vector3 Point2;

    public Vector3 Normal1;
    public Vector3 Normal2;

    public ushort Point1Id;
    public ushort Point2Id;

    public int? GetSize()
    {
        return sizeof(byte)
            + sizeof(ushort) + sizeof(byte)
            + sizeof(byte)
            + Tracker1.GetSize()
            + Tracker2.GetSize()
            + SerializedTransform.Size * 2
            + sizeof(float) * 3 * 4
            + sizeof(ushort) * 2;
    }

    public void Serialize(INetSerializer serializer)
    {
        var encodedPoint1 = NetworkTransformManager.EncodePosition(Point1);
        var encodedPoint2 = NetworkTransformManager.EncodePosition(Point2);

        serializer.SerializeValue(ref SmallID);
        serializer.SerializeValue(ref ConstrainerID);
        serializer.SerializeValue(ref Mode, Precision.OneByte);

        serializer.SerializeValue(ref Tracker1);
        serializer.SerializeValue(ref Tracker2);

        serializer.SerializeValue(ref Tracker1Transform);
        serializer.SerializeValue(ref Tracker2Transform);

        serializer.SerializeValue(ref encodedPoint1);
        serializer.SerializeValue(ref encodedPoint2);

        serializer.SerializeValue(ref Normal1);
        serializer.SerializeValue(ref Normal2);

        serializer.SerializeValue(ref Point1Id);
        serializer.SerializeValue(ref Point2Id);

        if (serializer.IsReader)
        {
            Point1 = NetworkTransformManager.DecodePosition(encodedPoint1);
            Point2 = NetworkTransformManager.DecodePosition(encodedPoint2);
        }
    }

    public static ConstraintCreateData Create(byte smallId, ushort? constrainerId, ConstrainerPointPair pair)
    {
        return new ConstraintCreateData()
        {
            SmallID = smallId,
            ConstrainerID = constrainerId,
            Mode = pair.mode,
            Tracker1 = new GameObjectReference(pair.go1),
            Tracker2 = new GameObjectReference(pair.go2),
            Tracker1Transform = new SerializedTransform(pair.go1.transform),
            Tracker2Transform = new SerializedTransform(pair.go2.transform),
            Point1 = pair.point1,
            Point2 = pair.point2,
            Normal1 = pair.normal1,
            Normal2 = pair.normal2,

            // These are unknown by the client, but are set by the server
            Point1Id = 0,
            Point2Id = 0,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ConstraintCreateMessage : ModuleMessageHandler
{
    public const int MaxConstraintsPerSecond = 4;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstraintCreateData>();

        var constrainerEntity = data.ConstrainerID.HasValue ? NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.ConstrainerID.Value) : null;
        bool hasConstrainer = constrainerEntity != null;

        // Send message to other clients if server
        // NOTE: OLD RELAY SYSTEM! REPLACE!
        if (received.IsServerHandled)
        {
            // If the player isn't hosting a level, limit the amount of constraints per second
            if (!NetworkSceneManager.PlayerIsLevelHost(PlayerIDManager.GetPlayerID(received.Sender.Value)))
            {
                var activity = LimitedActivityManager.GetTracker(nameof(ConstraintCreateMessage)).GetActivity(received.Sender.Value);

                activity.Increment();

                if (activity.Counter > MaxConstraintsPerSecond)
                {
                    FusionLogger.Warn($"Blocking Player {received.Sender.Value}'s constraint creation because they have tried to spawn {activity.Counter} entities in one second!");
                    return;
                }
            }

            // Make sure we have a constrainer server side (and it's being held)
            if (hasConstrainer)
            {
                // Recreate the message so we can assign server-side sync ids
                data.Point1Id = NetworkEntityManager.IDManager.RegisteredEntities.AllocateNewID();
                data.Point2Id = NetworkEntityManager.IDManager.RegisteredEntities.AllocateNewID();

                MessageRelay.RelayModule<ConstraintCreateMessage, ConstraintCreateData>(data, CommonMessageRoutes.ReliableToClients);
            }

            return;
        }

        data.Tracker1.HookGameObjectFound(OnFirstTrackerFound);

        void OnFirstTrackerFound(GameObject tracker1)
        {
            data.Tracker2.HookGameObjectFound(OnSecondTrackerFound);
        }

        void OnSecondTrackerFound(GameObject tracker2)
        {
            ConstrainerUtilities.HookConstrainerCreated(OnConstrainerReady);
        }

        void OnConstrainerReady()
        {
            bool tracker1HasPlayer = TryGetPlayer(data.Tracker1, out var tracker1Player);
            bool tracker2HasPlayer = TryGetPlayer(data.Tracker2, out var tracker2Player);

            if (!ConstrainerUtilities.PlayerConstraintsEnabled)
            {
                if (tracker1HasPlayer || tracker2HasPlayer)
                {
                    return;
                }
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
            comp.mode = data.Mode;

            // Setup points
            comp._point1 = data.Point1;
            comp._point2 = data.Point2;

            comp._normal1 = data.Normal1;
            comp._normal2 = data.Normal2;

            // Setup gameobjects
            comp._gO1 = data.Tracker1.GameObject;
            comp._gO2 = data.Tracker2.GameObject;
            comp._mb1 = data.Tracker1.Body;
            comp._mb2 = data.Tracker2.Body;

            // Store positions
            Transform tran1 = comp._gO1.transform;
            Transform tran2 = comp._gO2.transform;

            Vector3 go1Pos = tran1.position;
            Quaternion go1Rot = tran1.rotation;

            Vector3 go2Pos = tran2.position;
            Quaternion go2Rot = tran2.rotation;

            // Force positions
            tran1.SetPositionAndRotation(data.Tracker1Transform.position, data.Tracker1Transform.rotation);
            tran2.SetPositionAndRotation(data.Tracker2Transform.position, data.Tracker2Transform.rotation);

            // Create the constraint
            ConstrainerPatches.IsReceivingConstraints = true;
            ConstrainerPatches.FirstId = data.Point1Id;
            ConstrainerPatches.SecondId = data.Point2Id;

            if (hasConstrainer)
            {
                comp.LineMaterial = syncedComp.LineMaterial;
            }

            comp.PrimaryButtonUp();

            ConstrainerPatches.FirstId = 0;
            ConstrainerPatches.SecondId = 0;
            ConstrainerPatches.IsReceivingConstraints = false;

            // Reset positions
            tran1.SetPositionAndRotation(go1Pos, go1Rot);
            tran2.SetPositionAndRotation(go2Pos, go2Rot);

            // Events when the constrainer is from another player
            if (data.SmallID != PlayerIDManager.LocalSmallID)
            {
                if (hasConstrainer)
                {
                    // Play sound
                    syncedComp.sfx.GravLocked();
                    syncedComp.sfx.Release();
                }

                // Check for host constraint achievement
                if (data.SmallID == PlayerIDManager.HostSmallID && AchievementManager.TryGetAchievement<ClassStruggle>(out var achievement))
                {
                    bool tracker1IsSelf = tracker1HasPlayer && tracker1Player.NetworkEntity.IsOwner;
                    bool tracker2IsSelf = tracker2HasPlayer && tracker2Player.NetworkEntity.IsOwner;

                    if (!achievement.IsComplete && (tracker1IsSelf || tracker2IsSelf))
                    {
                        achievement.IncrementTask();
                    }
                }
            }
        }
    }

    private static bool TryGetPlayer(GameObjectReference reference, out NetworkPlayer player)
    {
        player = null;

        if (!reference.Entity.HasValue)
        {
            return false;
        }

        if (!reference.Entity.Value.TryGetEntity(out var networkEntity))
        {
            return false;
        }

        player = networkEntity.GetExtender<NetworkPlayer>();

        return player != null;
    }
}