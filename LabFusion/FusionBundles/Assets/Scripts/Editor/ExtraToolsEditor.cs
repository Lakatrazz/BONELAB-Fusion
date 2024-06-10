using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking.Types;

public class ExtraToolsEditor : EditorWindow
{
    public const BuildAssetBundleOptions BundleOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.AssetBundleStripUnityVersion;

    [MenuItem("BONELAB Fusion/Build/AssetBundles (Windows)")]
    static void BuildAllAssetBundlesWindows()
    {
        string assetBundleDirectory = "Assets/AssetBundles/StandaloneWindows64";
        Directory.CreateDirectory(assetBundleDirectory);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BundleOptions, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("BONELAB Fusion/Build/AssetBundles (Android)")]
    static void BuildAllAssetBundlesAndroid()
    {
        string assetBundleDirectory = "Assets/AssetBundles/Android";
        Directory.CreateDirectory(assetBundleDirectory);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BundleOptions, BuildTarget.Android);
    }

    [MenuItem("BONELAB Fusion/Build/AssetBundles (Both)")]
    static void BuildBothAssetBundles() {
        BuildAllAssetBundlesWindows();
        BuildAllAssetBundlesAndroid();
    }

    [MenuItem("BONELAB Fusion/Platform/Windows")]
    static void SwitchToWindows() {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("BONELAB Fusion/Platform/Android")]
    static void SwitchToAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
    }

    [MenuItem("BONELAB Fusion/Remove All AssetBundles")]
    static void RemoveAssetBundles() {
        var names = AssetDatabase.GetAllAssetBundleNames();
        foreach (string name in names)
            AssetDatabase.RemoveAssetBundleName(name, true);
    }

    [MenuItem("BONELAB Fusion/Copy AssetBundles To Resources")]
    static void CopyAssetBundles() {
        var assetsPath = Application.dataPath;
        var builtBundlesPath = Path.Combine(assetsPath, "AssetBundles");
        var fusionBundlesPath = Path.Combine(assetsPath, @"..\..\dependencies\resources\bundles");

        var allDirectories = Directory.GetDirectories(builtBundlesPath, "*", SearchOption.AllDirectories);

        foreach (string directory in allDirectories) {
            Directory.CreateDirectory(directory.Replace(builtBundlesPath, fusionBundlesPath));
        }

        var allFiles = Directory.GetFiles(builtBundlesPath, "*", SearchOption.AllDirectories);

        int count = 0;
        foreach (string file in allFiles) {
            if (Path.GetExtension(file) == ".fusion") {
                File.Copy(file, file.Replace(builtBundlesPath, fusionBundlesPath), true);
                count++;
            }
            File.Delete(file);
        }

        Debug.Log($"Done copying {count} files!");
    }
}
