using System;

namespace LabFusion.Utilities
{
    internal static class FusionLogger
    {
        internal static void Log(string txt, ConsoleColor txt_color = ConsoleColor.White)
        {
            FusionMod.Instance.LoggerInstance.Msg(txt_color, txt);
        }

        internal static void Log(object obj, ConsoleColor txt_color = ConsoleColor.White)
        {
            FusionMod.Instance.LoggerInstance.Msg(txt_color, obj);
        }

        internal static void Warn(string txt)
        {
            FusionMod.Instance.LoggerInstance.Warning(txt);
        }

        internal static void Warn(object obj)
        {
            FusionMod.Instance.LoggerInstance.Warning(obj);
        }

        internal static void Error(string txt)
        {
            FusionMod.Instance.LoggerInstance.Error(txt);
        }

        internal static void Error(object obj)
        {
            FusionMod.Instance.LoggerInstance.Error(obj);
        }

        internal static void LogException(string task, Exception e) {
            Error($"Failed {task} with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }
}
