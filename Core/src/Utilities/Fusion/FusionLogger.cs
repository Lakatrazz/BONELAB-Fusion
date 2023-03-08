using System;
using System.Runtime.CompilerServices;

namespace LabFusion.Utilities
{
    internal static class FusionLogger
    {
        internal static void LogLine([CallerLineNumber] int lineNumber = default) {
#if DEBUG
            Log($"DEBUG: Line {lineNumber}", ConsoleColor.Cyan);
#else
            Log($"FusionLogger.LogLine is only for debugging! Please remove from line {lineNumber}", ConsoleColor.Red);
#endif
        }

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

        internal static void ErrorLine(string txt, [CallerLineNumber] int lineNumber = default)
        {
            FusionMod.Instance.LoggerInstance.Error($"{txt} - Line: {lineNumber}");
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
