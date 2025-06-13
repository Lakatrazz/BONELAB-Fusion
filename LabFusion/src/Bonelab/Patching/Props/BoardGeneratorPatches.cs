using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.Bonelab.Messages;
using LabFusion.Bonelab.Extenders;
using LabFusion.Marrow.Extenders;

using UnityEngine;

using Random = UnityEngine.Random;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(BoardGenerator._BoardSpawnerAsync_d__29))]
public static class BoardSpawnerAsyncPatches
{
    public static Vector3 GetBoardScale(float distance)
    {
        return new Vector3(Mathf.Min(distance, 1f), Mathf.Min(distance, 1f), distance); ;
    }

    public static BoardPointData GetPointData(Vector3 point, MarrowBody body)
    {
        bool hasBody = false;
        ushort entityId = 0;
        ushort bodyIndex = 0;

        if (body != null && MarrowBodyExtender.Cache.TryGet(body, out var entity))
        {
            hasBody = true;
            entityId = entity.ID;

            var bodyExtender = entity.GetExtender<MarrowBodyExtender>();
            bodyIndex = bodyExtender.GetIndex(body).Value;
        }

        return new BoardPointData()
        {
            Point = point,
            HasBody = hasBody,
            EntityID = entityId,
            BodyIndex = bodyIndex,
        };
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BoardGenerator._BoardSpawnerAsync_d__29.MoveNext))]
    public static bool MoveNext(BoardGenerator._BoardSpawnerAsync_d__29 __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var boardGenerator = __instance.__4__this;

        if (!BoardGeneratorExtender.Cache.TryGet(boardGenerator, out var generatorEntity))
        {
            return true;
        }

        if (!generatorEntity.IsOwner)
        {
            return false;
        }

        // Replicate board gun logic server side
        var spawnables = boardGenerator.boardSpawnable;
        var randomSpawnable = spawnables[Random.RandomRangeInt(0, spawnables.Count)];

        var start = boardGenerator.firstPoint;
        var end = boardGenerator.EndPoint;

        var middle = (start + end) * 0.5f;

        var distance = end - start;

        // Misfire?
        if (distance.magnitude < 0.1f)
        {
            boardGenerator.PlaySFX(boardGenerator.misfireSFX, boardGenerator.FirePoint.position);
            return false;
        }

        var direction = distance.normalized;

        var rotation = Quaternion.LookRotation(direction, boardGenerator.upDir);

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = randomSpawnable,
            Position = middle,
            Rotation = rotation,
            SpawnCallback = (info) =>
            {
                // Apply scale
                info.Spawned.transform.localScale = GetBoardScale(distance.magnitude);

                // Get the entity ids
                var boardId = info.Entity.ID;
                var boardGeneratorId = generatorEntity.ID;

                // Get points
                var firstPoint = GetPointData(boardGenerator.firstPoint, boardGenerator.FirstRb);
                var endPoint = GetPointData(boardGenerator.EndPoint, boardGenerator.EndRb);

                // Send the generator message
                var data = new BoardGeneratorData()
                {
                    OwnerID = PlayerIDManager.LocalSmallID,
                    BoardID = boardId,
                    BoardGeneratorID = boardGeneratorId,
                    FirstPoint = firstPoint,
                    EndPoint = endPoint,
                };

                MessageRelay.RelayModule<BoardGeneratorMessage, BoardGeneratorData>(data, CommonMessageRoutes.ReliableToClients);
            }
        });

        return false;
    }
}