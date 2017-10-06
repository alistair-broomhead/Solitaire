using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    private static string assetBundlePath = Path.Combine(Application.dataPath, "AssetBundles");
    private static BuildTarget[] platforms = new BuildTarget[] {
        BuildTarget.WebGL,
        BuildTarget.Android,
        BuildTarget.StandaloneWindows64,
        BuildTarget.StandaloneOSXIntel64,
        BuildTarget.StandaloneLinux64,
    };

    static void Rm(string dirName)
    {
        var info = new DirectoryInfo(dirName);

        if (info.Exists)
            info.Delete(true);
    }
    static void CopyDir(string fromDir, string toDir)
    {
        var fromInfo = new DirectoryInfo(fromDir);

        if (!fromInfo.Exists)
            return;

        if (!Directory.Exists(toDir))
            Directory.CreateDirectory(toDir);

        foreach (var file in fromInfo.GetFiles())
            file.CopyTo(Path.Combine(toDir, file.Name), true);

        foreach (var subDir in fromInfo.GetDirectories())
            CopyDir(subDir.FullName, Path.Combine(toDir, subDir.Name));
    }

    static string PlatformDir()
    {
        return PlatformDir(EditorUserBuildSettings.activeBuildTarget);
    }
    static string PlatformDir(BuildTarget platform)
    {
        return Path.Combine(assetBundlePath, platform.ToString());
    }
    
    [MenuItem("Build/AssetBundles")]
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        Rm(PlatformDir());

        Rm(Application.streamingAssetsPath);
        
        foreach (var platform in platforms)
        {
            Rm(PlatformDir(platform));
            EnsureBuilt(platform);
        }

        EnsureBuilt();
    }

    [MenuItem("Build/Make sure there's an AssetBundle for the current build target")]
    static void EnsureBuilt() {
        EnsureBuilt(EditorUserBuildSettings.activeBuildTarget);
    }
    static void EnsureBuilt(BuildTarget platform)
    {
        string directory = PlatformDir(platform);

        if (!Directory.Exists(directory))
            BuildPlatform(platform, directory);

        if (platform == EditorUserBuildSettings.activeBuildTarget)
            CopyDir(directory, Application.streamingAssetsPath);
    }

    static void BuildPlatform(BuildTarget platform, string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        BuildPipeline.BuildAssetBundles(
            directory,
            BuildAssetBundleOptions.None,
            platform
        );
    }

}