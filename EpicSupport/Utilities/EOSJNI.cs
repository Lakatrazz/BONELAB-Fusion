using System.Reflection;
using System.Runtime.InteropServices;
using JNISharp.NativeInterface;
using LabFusion.Data;

namespace MarrowFusion.Epic.Utilities;

internal static class EOSJNI
{
    private static readonly BindingFlags allBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

    private static IntPtr javaVM;

    private static JClass eosSdkClass;
    private static JClass unityPlayerClass;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int JNI_OnLoadDelegate(IntPtr javaVM, IntPtr reserved);

    internal static void Initialize()
    {
        if (javaVM != IntPtr.Zero)
            return;

        ExtractDexFiles();

        foreach (string dexFile in Directory.GetFiles(PersistentData.GetPath(""), "*.dex", SearchOption.TopDirectoryOnly))
        {
            InjectDex(dexFile);
        }

        eosSdkClass = JNI.FindClass("com/epicgames/mobile/eossdk/EOSSDK");

        LoadJNI();
        InitializeEOS();
    }

    private static void LoadJNI()
    {
        ResolveJavaVM();
        InvokeNativeJNIOnLoad();
        LoadEOSNativeLibrary();
    }

    private static void ResolveJavaVM()
    {
        Type jniType = Type.GetType("JNISharp.NativeInterface.JNI, JNISharp");

        FieldInfo lastVmPtrField = jniType.GetField("lastVmPtr", allBindingFlags);

        javaVM = (IntPtr)lastVmPtrField.GetValue(null);
    }

    private static void InvokeNativeJNIOnLoad()
    {
        IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(EOSSDKLoader.LibraryPtr, "JNI_OnLoad");

        var onLoad = Marshal.GetDelegateForFunctionPointer<JNI_OnLoadDelegate>(onLoadPtr);

        int result = onLoad(javaVM, IntPtr.Zero);
    }

    private static void LoadEOSNativeLibrary()
    {
        unityPlayerClass = JNI.FindClass("com/unity3d/player/UnityPlayer");

        JFieldID activityField = JNI.GetStaticFieldID(unityPlayerClass, "currentActivity", "Landroid/app/Activity;");

        JObject activity = JNI.GetStaticObjectField<JObject>(unityPlayerClass, activityField);

        JClass contextClass = JNI.FindClass("android/content/Context");

        JMethodID getPackageNameMethod = JNI.GetMethodID(contextClass, "getPackageName", "()Ljava/lang/String;");

        JString packageNameObject = JNI.CallObjectMethod<JString>(activity, getPackageNameMethod);
        
        string libraryPath = $"/data/data/{packageNameObject.GetString()}/libEOSSDK.so";

        JClass systemClass = JNI.FindClass("java/lang/System");

        JMethodID loadMethod = JNI.GetStaticMethodID(systemClass, "load", "(Ljava/lang/String;)V");

        JNI.CallStaticVoidMethod(systemClass, loadMethod, JNI.NewString(libraryPath));
    }

    private static void InitializeEOS()
    {
        JFieldID activityField = JNI.GetStaticFieldID(unityPlayerClass, "currentActivity", "Landroid/app/Activity;");

        JObject activity = JNI.GetStaticObjectField<JObject>(unityPlayerClass, activityField);

        JMethodID initMethod = eosSdkClass.GetStaticMethodID("init", "(Landroid/app/Activity;)V");

        JNI.CallStaticVoidMethod(eosSdkClass, initMethod, activity);
    }

    private static void ExtractDexFiles()
    {
        const string resourcePrefix = "EpicSupport.dependencies.resources.dex.";

        Assembly assembly = EpicModule.ModuleAssembly;

        string[] resources = assembly.GetManifestResourceNames().Where(x =>
        x.StartsWith(
            resourcePrefix,
            StringComparison.OrdinalIgnoreCase) &&
        x.EndsWith(
            ".dex",
            StringComparison.OrdinalIgnoreCase))
        .ToArray();

        foreach (string resource in resources)
        {
            string fileName = resource[resourcePrefix.Length..];

            string outputPath = PersistentData.GetPath(fileName);

            byte[] bytes = EmbeddedResource.LoadBytesFromAssembly(assembly, resource);

            File.WriteAllBytes(outputPath, bytes);
        }
    }

    private static void InjectDex(string dexPath)
    {
        JClass threadClass = JNI.FindClass("java/lang/Thread");

        JMethodID currentThreadMethod = threadClass.GetStaticMethodID("currentThread", "()Ljava/lang/Thread;");

        JObject currentThread = JNI.CallStaticObjectMethod<JObject>(threadClass, currentThreadMethod);

        JMethodID getContextClassLoaderMethod = threadClass.GetMethodID("getContextClassLoader", "()Ljava/lang/ClassLoader;");

        JObject classLoader = JNI.CallObjectMethod<JObject>(currentThread, getContextClassLoaderMethod);

        JClass fileClass = JNI.FindClass("java/io/File");

        JMethodID fileConstructor = fileClass.GetMethodID("<init>", "(Ljava/lang/String;)V");

        JObject dexPathString = JNI.NewString(dexPath);

        JObject dexFile = JNI.NewObject<JObject>(fileClass, fileConstructor, new JValue(dexPathString));

        JClass baseDexClassLoaderClass = JNI.FindClass("dalvik/system/BaseDexClassLoader");

        JFieldID pathListField = baseDexClassLoaderClass.GetFieldID("pathList", "Ldalvik/system/DexPathList;");

        JObject pathList = JNI.GetObjectField<JObject>(classLoader, pathListField);

        JClass dexPathListClass = JNI.FindClass("dalvik/system/DexPathList");

        JMethodID addDexPathMethod = dexPathListClass.GetMethodID("addDexPath", "(Ljava/lang/String;Ljava/io/File;)V");

        JNI.CallVoidMethod(pathList, addDexPathMethod, new JValue(dexPathString), new JValue(dexFile));

        JNI.CheckExceptionAndThrow();
    }
}