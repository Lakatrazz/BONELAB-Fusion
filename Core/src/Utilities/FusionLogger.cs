using System;

namespace LabFusion.Utilities
{
    public static class FusionLogger
    {
        public static void Log(string txt, ConsoleColor txt_color = ConsoleColor.White)
        {
            FusionMod.Instance.LoggerInstance.Msg(txt_color, txt);
        }

        public static void Log(object obj, ConsoleColor txt_color = ConsoleColor.White)
        {
            FusionMod.Instance.LoggerInstance.Msg(txt_color, obj);
        }

        public static void Warn(string txt)
        {
            FusionMod.Instance.LoggerInstance.Warning(txt);
        }

        public static void Warn(object obj)
        {
            FusionMod.Instance.LoggerInstance.Warning(obj);
        }

        public static void Error(string txt)
        {
            FusionMod.Instance.LoggerInstance.Error(txt);
        }

        public static void Error(object obj)
        {
            FusionMod.Instance.LoggerInstance.Error(obj);
        }
    }
}
