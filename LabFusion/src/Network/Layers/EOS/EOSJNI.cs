using JNISharp.NativeInterface;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    internal class EOSJNI
    {
        internal static void EOS_Init()
        {
            JClass playerClass = JNI.FindClass("com/unity3d/player/UnityPlayer");
            JFieldID currentActivityField = JNI.GetStaticFieldID(playerClass, "currentActivity", "Landroid/app/Activity;");
            JObject currentActivity = JNI.GetStaticObjectField<JObject>(playerClass, currentActivityField);

            JClass eosClass = JNI.FindClass("com/epicgames/mobile/eossdk/EOSSDK");

            JMethodID initMethod = eosClass.GetStaticMethodID("init", "(Landroid/app/Activity;)V");

            JNI.CallStaticVoidMethod(eosClass, initMethod, currentActivity);

            JMethodID versionMethod = eosClass.GetStaticMethodID("GetOSVersion", "()Ljava/lang/String;");

            string version = JNI.CallStaticObjectMethod<JString>(eosClass, versionMethod).GetString();
            FusionLogger.Log($"EOS SDK initialized with version: {version}");
        }
    }
}
