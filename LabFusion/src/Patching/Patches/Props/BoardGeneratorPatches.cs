﻿using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;

using UnityEngine;

using Random = UnityEngine.Random;

namespace LabFusion.Patching;

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
            entityId = entity.Id;

            var bodyExtender = entity.GetExtender<MarrowBodyExtender>();
            bodyIndex = bodyExtender.GetIndex(body).Value;
        }

        return new BoardPointData()
        {
            point = point,
            hasBody = hasBody,
            entityId = entityId,
            bodyIndex = bodyIndex,
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
            spawnable = randomSpawnable,
            position = middle,
            rotation = rotation,
            spawnCallback = (info) =>
            {
                // Apply scale
                info.spawned.transform.localScale = GetBoardScale(distance.magnitude);

                // Get the entity ids
                var boardId = info.entity.Id;
                var boardGeneratorId = generatorEntity.Id;

                // Get points
                var firstPoint = GetPointData(boardGenerator.firstPoint, boardGenerator.FirstRb);
                var endPoint = GetPointData(boardGenerator.EndPoint, boardGenerator.EndRb);

                // Send the generator message
                using var writer = FusionWriter.Create(BoardGeneratorData.Size);
                var data = BoardGeneratorData.Create(PlayerIdManager.LocalSmallId, boardId, boardGeneratorId, firstPoint, endPoint);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.BoardGenerator, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        });

        return false;
    }
}