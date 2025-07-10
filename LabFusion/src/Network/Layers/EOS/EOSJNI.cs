using JNISharp.NativeInterface;

using LabFusion.Data;

using System.Runtime.InteropServices;

namespace LabFusion.Utilities;

internal class EOSJNI
{
    private static JClass EOSSDK { get; set; } = null;

    internal static IntPtr JavaVM { get; private set; } = IntPtr.Zero;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int JNI_OnLoadDelegate(IntPtr javaVM, IntPtr reserved);

    private static bool _initialized = false;

    internal static void JNI_OnLoad()
    {
        var jniType = Type.GetType("JNISharp.NativeInterface.JNI, JNISharp");
        if (jniType == null)
        {
            FusionLogger.Error("JNISharp.NativeInterface.JNI type not found!");
            return;
        }

        var lastVmPtrField = jniType.GetField("lastVmPtr", HarmonyLib.AccessTools.all);
        if (lastVmPtrField == null)
        {
            FusionLogger.Error("lastVmPtr field not found in JNI type!");
            return;
        }

        JavaVM = (IntPtr)lastVmPtrField.GetValue(null);
        if (JavaVM == IntPtr.Zero)
        {
            FusionLogger.Error("Failed to get Java VM pointer from JNISharp!");
            return;
        }

        IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(EOSSDKLoader._libraryPtr, "JNI_OnLoad");

        var onLoad = Marshal.GetDelegateForFunctionPointer<JNI_OnLoadDelegate>(onLoadPtr);
        int result = onLoad(JavaVM, IntPtr.Zero);

#if DEBUG
        // jni version 1.6 in hex = good
        FusionLogger.Log($"JNI_OnLoad returned: {result}");
#endif

        JClass systemClass = JNISharp.NativeInterface.JNI.FindClass("java/lang/System");
        JMethodID loadMethod = JNISharp.NativeInterface.JNI.GetStaticMethodID(systemClass, "load", "(Ljava/lang/String;)V");
        var libPath = JNI.NewString("/data/data/com.StressLevelZero.BONELAB/libEOSSDK.so");
        JNI.CallStaticVoidMethod(systemClass, loadMethod, libPath);
        if (JNI.ExceptionCheck())
        {
            JNI.ExceptionDescribe();
            FusionLogger.Error("Failed to call loadLibrary for libEOSSDK!");
        }
    }

    internal static void EOS_Init()
    {
        if (_initialized)
            return;

        if (!isEOSPatched())
            return;    

        JClass playerClass = JNI.FindClass("com/unity3d/player/UnityPlayer");
        JFieldID currentActivityField = JNI.GetStaticFieldID(playerClass, "currentActivity", "Landroid/app/Activity;");
        JObject currentActivity = JNI.GetStaticObjectField<JObject>(playerClass, currentActivityField);
        if (!currentActivity.Valid())
        {
            FusionLogger.Error("Failed to get current activity from UnityPlayer! EOS SDK initialization aborted.");
            return;
        }

        JMethodID initMethod = EOSSDK.GetStaticMethodID("init", "(Landroid/app/Activity;)V");

        JNI.CallStaticVoidMethod(EOSSDK, initMethod, currentActivity);
        if (JNI.ExceptionCheck())
        {
            FusionLogger.Error("Failed to initialize the EOS SDK!");
        }
        else
            FusionLogger.Log("EOS SDK initialized successfully in Java.");

        _initialized = true;
    }

    private static bool isEOSPatched()
    {
        EOSSDK = JNI.FindClass("com/epicgames/mobile/eossdk/EOSSDK");
        if (!EOSSDK.Valid())
        {
            FusionLogger.Error("Failed to find EOSSDK class!");
            FusionLogger.Error("Did the user install the EOS SDK plugin when installing Lemonloader?");
            
            return false;
        }
        else
            return true;
    }
}
