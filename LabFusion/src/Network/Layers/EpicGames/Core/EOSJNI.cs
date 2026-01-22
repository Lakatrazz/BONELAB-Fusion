using LabFusion.Data;
using LabFusion.Utilities;

using JNISharp.NativeInterface;

using System.Runtime.InteropServices;

namespace LabFusion.Network.EpicGames;

internal static class EOSJNI
{
    private static IntPtr _javaVM = IntPtr.Zero;

    private static JClass _eosSdkClass;
    private static JClass _unityPlayerClass;

    private const string EOS_LIBRARY_PATH = "/data/data/com.StressLevelZero.BONELAB/libEOSSDK.so";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int JNI_OnLoadDelegate(IntPtr javaVM, IntPtr reserved);

    internal static void Initialize()
    {
        if (IsInitialized())
            return;

        if (!ValidatePatch())
            return;

        if (!LoadJNI())
            return;

        if (!InitializeEOS())
            return;
    }

    private static bool IsInitialized()
    {
        return _javaVM != IntPtr.Zero;
    }
    
    private static bool ValidatePatch()
    {
        _eosSdkClass = JNI.FindClass("com/epicgames/mobile/eossdk/EOSSDK");

        if (_eosSdkClass.Valid())
            return true;

        FusionLogger.Error("EOSSDK class not found!");
        FusionLogger.Error("Did the user use the plugin when installing LemonLoader?");
        return false;
    }
    
    private static bool LoadJNI()
    {
        if (!ResolveJavaVM())
            return false;

        if (!InvokeNativeJNIOnLoad())
            return false;

        if (!LoadEOSNativeLibrary())
            return false;

        return true;
    }

    private static bool ResolveJavaVM()
    {
        var jniType = Type.GetType("JNISharp.NativeInterface.JNI, JNISharp");
        if (jniType == null)
        {
            FusionLogger.Error("JNISharp.NativeInterface.JNI type not found!");
            return false;
        }

        var lastVmPtrField = jniType.GetField("lastVmPtr", HarmonyLib.AccessTools.all);

        if (lastVmPtrField == null)
        {
            FusionLogger.Error("JNISharp lastVmPtr field not found!");
            return false;
        }

        _javaVM = (IntPtr)lastVmPtrField.GetValue(null);

        if (_javaVM == IntPtr.Zero)
        {
            FusionLogger.Error("Failed to retrieve JavaVM pointer from JNISharp!");
            return false;
        }

        return true;
    }

    private static bool InvokeNativeJNIOnLoad()
    {
        IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(EOSSDKLoader.LibraryPtr, "JNI_OnLoad");

        if (onLoadPtr == IntPtr.Zero)
        {
            FusionLogger.Error("JNI_OnLoad export not found in EOS SDK!");
            return false;
        }

        var onLoad = Marshal.GetDelegateForFunctionPointer<JNI_OnLoadDelegate>(onLoadPtr);
        int result = onLoad(_javaVM, IntPtr.Zero);

#if DEBUG
        // JNI 1.6 = 0x00010006
        FusionLogger.Log($"JNI_OnLoad returned: 0x{result:X}");
#endif

        return true;
    }

    private static bool LoadEOSNativeLibrary()
    {
        JClass systemClass = JNI.FindClass("java/lang/System");
        JMethodID loadMethod = JNI.GetStaticMethodID(systemClass, "load", "(Ljava/lang/String;)V");

        JObject libPath = JNI.NewString(EOS_LIBRARY_PATH);

        JNI.CallStaticVoidMethod(systemClass, loadMethod, libPath);

        if (!JNI.ExceptionCheck())
            return true;

        JNI.ExceptionDescribe();
        FusionLogger.Error("Failed to load libEOSSDK.so via System.load!");
        return false;
    }
    
    private static bool InitializeEOS()
    {
        _unityPlayerClass = JNI.FindClass("com/unity3d/player/UnityPlayer");

        JFieldID activityField = JNI.GetStaticFieldID(_unityPlayerClass, "currentActivity", "Landroid/app/Activity;");

        JObject activity = JNI.GetStaticObjectField<JObject>(_unityPlayerClass, activityField);

        if (!activity.Valid())
        {
            FusionLogger.Error("Failed to retrieve UnityPlayer.currentActivity!");
            return false;
        }

        JMethodID initMethod = _eosSdkClass.GetStaticMethodID("init", "(Landroid/app/Activity;)V");

        JNI.CallStaticVoidMethod(_eosSdkClass, initMethod, activity);

        if (!JNI.ExceptionCheck())
            return true;

        JNI.ExceptionDescribe();
        FusionLogger.Error("EOS SDK initialization failed!");
        return false;
    }
}
