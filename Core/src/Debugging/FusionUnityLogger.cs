#if DEBUG
using LabFusion.Utilities;

using System;
using System.Diagnostics;

using UnityEngine;

namespace LabFusion.Debugging
{
    public class FusionUnityLogger {
        public const bool EnableUnityLogs = false;
        public const bool EnableArrayResizeLogs = false;

        public static void OnInitializeMelon() {
            if (EnableUnityLogs) {
#pragma warning disable CS0162 // Unreachable code detected
                Application.add_logMessageReceived((Application.LogCallback)((a, b, c) => {
                    switch (c) {
                        default:
                        case LogType.Log:
                            FusionLogger.Log($"UNITY -> {a}");
                            break;
                        case LogType.Warning:
                            FusionLogger.Warn($"UNITY -> {a}");
                            break;
                        case LogType.Error:
                            FusionLogger.Error($"UNITY -> {a}");
                            break;
                    }
                }));
#pragma warning restore CS0162 // Unreachable code detected
            }
        }
    }
}
#endif