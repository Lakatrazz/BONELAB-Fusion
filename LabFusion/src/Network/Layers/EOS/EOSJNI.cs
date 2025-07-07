using JNISharp.NativeInterface;

using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSJNI
{
	private static bool _initialized = false;

    internal static void EOS_Init()
	{
		if (_initialized)
			return;

        JClass playerClass = JNI.FindClass("com/unity3d/player/UnityPlayer");
		JFieldID currentActivityField = JNI.GetStaticFieldID(playerClass, "currentActivity", "Landroid/app/Activity;");
		JObject currentActivity = JNI.GetStaticObjectField<JObject>(playerClass, currentActivityField);
		if (!currentActivity.Valid())
		{
			FusionLogger.Error("Failed to get current activity from UnityPlayer! EOS SDK initialization aborted.");
			return;
        }

		JClass eosClass = JNI.FindClass("com/epicgames/mobile/eossdk/EOSSDK");
		if (!eosClass.Valid())
		{
			FusionLogger.Error("Failed to find EOSSDK class! EOS SDK initialization aborted.");
			FusionLogger.Error("Did the user install the EOS SDK plugin when installing Lemonloader?");
            return;
        }

        JMethodID initMethod = eosClass.GetStaticMethodID("init", "(Landroid/app/Activity;)V");

        JNI.CallStaticVoidMethod(eosClass, initMethod, currentActivity);
		if (JNI.ExceptionCheck())
		{
			FusionLogger.Error("Failed to initialize the EOS SDK!");
        }
		else
			FusionLogger.Log("EOS SDK initialized successfully in Java.");

        _initialized = true;
    }
}
