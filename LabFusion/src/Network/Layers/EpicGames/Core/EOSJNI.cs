using LabFusion.Data;
using LabFusion.Utilities;

using JNISharp.NativeInterface;

using System.Reflection;
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
        
        if (!RedirectDllImport("EOSSDK-Win64-Shipping.dll", "EOSSDK"))
            return;
        
        if (!ExtractDexFiles())
            return;
        
        if (!InjectDex(PersistentData.GetPath("util.dex")))
            return;

        if (!InjectDex(PersistentData.GetPath("androidx.dex")))
            return;
        
        if (!InjectDex(PersistentData.GetPath("eossdk.dex")))
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
        FusionLogger.Error("Did dex injection succeed?");
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

    private static bool ExtractDexFiles()
    {
        const string utilResourcePath = "LabFusion.dependencies.resources.dex.util.dex";
        const string androidxResourcePath = "LabFusion.dependencies.resources.dex.androidx.dex";
        const string eossdkResourcePath = "LabFusion.dependencies.resources.dex.eossdk.dex";

        string utilPath = PersistentData.GetPath("util.dex");
        string androidxPath = PersistentData.GetPath("androidx.dex");
        string eossdkPath = PersistentData.GetPath("eossdk.dex");
        
        File.WriteAllBytes(utilPath, EmbeddedResource.LoadBytesFromAssembly(FusionMod.FusionAssembly, utilResourcePath));
        File.WriteAllBytes(androidxPath, EmbeddedResource.LoadBytesFromAssembly(FusionMod.FusionAssembly, androidxResourcePath));
        File.WriteAllBytes(eossdkPath, EmbeddedResource.LoadBytesFromAssembly(FusionMod.FusionAssembly, eossdkResourcePath));

        if (!File.Exists(utilPath) || !File.Exists(androidxPath) || !File.Exists(eossdkPath))
            return false;

#if DEBUG
        FusionLogger.Log("Extracted dex files!");
#endif
        
        return true;
    }
    
    private static bool InjectDex(string dexPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dexPath))
            {
                FusionLogger.Log("DEX path is null or empty");
                return false;
            }

            if (!File.Exists(dexPath))
            {
                FusionLogger.Log($"DEX file not found: {dexPath}");
                return false;
            }

            var extension = Path.GetExtension(dexPath).ToLowerInvariant();
            if (extension != ".dex")
            {
                FusionLogger.Log($"Invalid file extension: {extension}. Expected .dex");
                return false;
            }
            
            var threadClass = JNI.FindClass("java/lang/Thread");
            var currentThreadMethod = threadClass.GetStaticMethodID("currentThread", "()Ljava/lang/Thread;");
            var currentThread = JNI.CallStaticObjectMethod<JObject>(threadClass, currentThreadMethod);
            
            var getContextClassLoaderMethod = threadClass.GetMethodID("getContextClassLoader", "()Ljava/lang/ClassLoader;");
            var classLoader = JNI.CallObjectMethod<JObject>(currentThread, getContextClassLoaderMethod);      
            
            var fileClass = JNI.FindClass("java/io/File");
            var fileConstructor = fileClass.GetMethodID("<init>", "(Ljava/lang/String;)V");
            var dexPathStr = JNI.NewString(dexPath);
            
            var dexFile = JNI.NewObject<JObject>(fileClass, fileConstructor, new JValue(dexPathStr));
            if (!dexFile.Valid())
            {
                FusionLogger.Log("Failed to create File object for DEX path");
                return false;
            }
            
            var baseDexClassLoaderClass = JNI.FindClass("dalvik/system/BaseDexClassLoader");
            var pathListField = baseDexClassLoaderClass.GetFieldID("pathList", "Ldalvik/system/DexPathList;");
            var pathList = JNI.GetObjectField<JObject>(classLoader, pathListField);
            var dexPathListClass = JNI.FindClass("dalvik/system/DexPathList");
            var addDexPathMethod = dexPathListClass.GetMethodID("addDexPath", "(Ljava/lang/String;Ljava/io/File;)V");
            
            JNI.CallVoidMethod(pathList, addDexPathMethod, new JValue(dexPathStr), new JValue(dexFile));
            
            JNI.CheckExceptionAndThrow();
#if DEBUG
            FusionLogger.Log($"Successfully loaded DEX file: {dexPath}");
#endif
            return true;
        }
        catch (JThrowableException jex)
        {
            FusionLogger.Error($"Java exception while loading DEX file: {jex.Throwable}");
            try
            {
                string message = jex.Throwable.GetMessage();
                FusionLogger.Error($"Exception message: {message}");
            }
            catch
            {
                FusionLogger.Error("Could not retrieve exception message");
            }
            return false;
        }
        catch (JNIResultException jniEx)
        {
            FusionLogger.Error($"JNI error while loading DEX file: {jniEx.Result}");
            return false;
        }
        catch (Exception ex)
        {
            FusionLogger.Error($"Unexpected error loading DEX file: {ex.GetType().Name} - {ex.Message}");
            FusionLogger.Log($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
    
    public static bool RedirectDllImport(string originalDll, string newDll)
    {
        IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) 
        {
            if (!libraryName.Equals(originalDll, StringComparison.OrdinalIgnoreCase)) return IntPtr.Zero;
            
            try
            {
                return NativeLibrary.Load(newDll, assembly, searchPath);
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        try
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ImportResolver);
            return true;
        }
        catch (Exception)
        {
            FusionLogger.Error("Failed to set DLLImport Resolver!");
            return false;
        }
    }
}
