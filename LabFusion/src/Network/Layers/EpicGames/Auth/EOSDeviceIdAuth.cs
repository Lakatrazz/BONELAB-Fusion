using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

using LabFusion.Utilities;

using System.Collections;

using UnityEngine;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles EOS Device ID creation and management.
/// </summary>
internal class EOSDeviceIdAuth
{
    public IEnumerator CreateDeviceIdAsync(Action<bool> onComplete)
    {
        var connect = EOSInterfaces.Connect;
        if (connect == null)
        {
            FusionLogger.Error("ConnectInterface is null when creating device ID");
            onComplete?.Invoke(false);
            yield break;
        }

        bool finished = false;
        bool success = false;

        var options = new CreateDeviceIdOptions
        {
            DeviceModel = GetDeviceModel(),
        };

        connect.CreateDeviceId(ref options, null, (ref CreateDeviceIdCallbackInfo data) =>
        {
            success = data.ResultCode == Result.Success ||
                      data.ResultCode == Result.DuplicateNotAllowed;

            if (!success)
            {
                FusionLogger.Error($"CreateDeviceId failed: {data.ResultCode}");
            }

            finished = true;
        });

        while (!finished)
            yield return null;

        onComplete?.Invoke(success);
    }

    private static string GetDeviceModel()
    {
        try
        {
            return SystemInfo.deviceModel ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}