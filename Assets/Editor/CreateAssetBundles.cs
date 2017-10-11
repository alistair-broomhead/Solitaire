using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class Constants
{
    public readonly static string assetBundlePath = Path.Combine(Application.dataPath, "AssetBundles");
    public readonly static string BuildPath = "Builds";
    public readonly static BuildTarget[] platforms = new BuildTarget[] {
        BuildTarget.Android,
        // BuildTarget.StandaloneWindows64,
        // BuildTarget.WebGL,
        // BuildTarget.StandaloneOSXIntel64,
        // BuildTarget.StandaloneLinux64,
    };
    
    public readonly static Dictionary<BuildTarget, string[]> packageNames = new Dictionary<BuildTarget, string[]>
    {
        {BuildTarget.Android, new string[]{"Android", ".apk"}},
        {BuildTarget.WebGL, new string[]{"WebGL", ""}},
        {BuildTarget.StandaloneWindows64, new string[]{ "Win64", ".exe"}},
        {BuildTarget.StandaloneOSXIntel64, new string[]{ "OSX64", ".app"}},
        {BuildTarget.StandaloneLinux64, new string[]{ "Linux64", ".bin"}},
    };
}

public static class DirUtils
{
    public static void RmStreamingBundle()
    {
        RmContent(Application.streamingAssetsPath);
    }
    public static void RmPlatformBundle()
    {
        RmPlatformBundle(EditorUserBuildSettings.activeBuildTarget);
    }
    public static void RmPlatformBundle(BuildTarget platform)
    {
        Rm(PlatformBundle(platform));
    }
    public static void Rm(string dirName)
    {
        var info = new DirectoryInfo(dirName);

        if (info.Exists)
            info.Delete(true);
    }
    public static void RmContent(string dirName)
    {
        if (dirName == null)
            return;

        var info = new DirectoryInfo(dirName);

        if (!info.Exists)
            return;

        foreach (var file in info.GetFiles())
            file.Delete();

        foreach (var dir in info.GetDirectories())
            dir.Delete(true);
    }
    public static void RmBuild(BuildTarget platform)
    {
        RmContent(PlatformBuildDir(platform));
    }
    public static void Cp(string fromDir, string toDir)
    {
        var fromInfo = new DirectoryInfo(fromDir);

        if (!fromInfo.Exists)
            return;

        Rm(toDir);

        if (!Directory.Exists(toDir))
            Directory.CreateDirectory(toDir);

        foreach (var file in fromInfo.GetFiles())
            file.CopyTo(Path.Combine(toDir, file.Name));

        foreach (var subDir in fromInfo.GetDirectories())
            Cp(subDir.FullName, Path.Combine(toDir, subDir.Name));
    }
    public static string PlatformBundle()
    {
        return PlatformBundle(EditorUserBuildSettings.activeBuildTarget);
    }
    public static string PlatformBundle(BuildTarget platform)
    {
        return Path.Combine(Constants.assetBundlePath, platform.ToString());
    }
    public static string PlatformBuildPath(BuildTarget platform)
    {
        if (!Constants.packageNames.ContainsKey(platform))
            return null;

        var platformDir = Path.Combine(Constants.BuildPath, Constants.packageNames[platform][0]);
        var packageName = PlayerSettings.productName + Constants.packageNames[platform][1];
        return Path.Combine(platformDir, packageName);
    }
    public static string PlatformBuildDir(BuildTarget platform)
    {
        if (!Constants.packageNames.ContainsKey(platform))
            return null;

        return Path.Combine(Constants.BuildPath, Constants.packageNames[platform][0]);
    }
}

public class CreateAssetBundles
{
    
    [MenuItem("Build/AssetBundles")]
    static void BuildAllAssetBundles()
    {
        DirUtils.RmPlatformBundle();
        
        foreach (var platform in Constants.platforms)
        {
            DirUtils.RmPlatformBundle(platform);
            EnsureBuilt(platform);
        }

        EnsureBuilt();
    }
    
    public static void EnsureBuilt() {
        EnsureBuilt(EditorUserBuildSettings.activeBuildTarget);
    }
    public static void EnsureBuilt(BuildTarget platform)
    {
        EnsureBuilt(platform, platform == EditorUserBuildSettings.activeBuildTarget);
    }
    public static void EnsureBuilt(BuildTarget platform, bool setStreamingAssets)
    {
        string directory = DirUtils.PlatformBundle(platform);

        bool mustBuild = !Directory.Exists(directory);
        bool platformChange = (platform != EditorUserBuildSettings.activeBuildTarget);

        if (mustBuild)
            BuildPlatform(platform, directory);

        if (setStreamingAssets && (mustBuild || platformChange))
        {
            var group = BuildPipeline.GetBuildTargetGroup(platform);
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, platform);
            DirUtils.RmStreamingBundle();
            DirUtils.Cp(directory, Application.streamingAssetsPath);
        }
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

public class ProjectUtils
{
    [MenuItem("Build/All")]
    public static void BuildAll ()
    {
        var scenes = Scenes;

        foreach(var platform in Constants.platforms)
            DirUtils.RmBuild(platform);

        foreach (var platform in Constants.platforms)
            if (platform == BuildTarget.Android)
            {
                BuildAndroidRelease();
                BuildAndroidDebug();
            }
            else
                BuildPlatform(platform, scenes);
        
        CreateAssetBundles.EnsureBuilt();
    }

    [MenuItem("Build/Rebuild")]
    public static void BuildPlatform()
    {
        CreateAssetBundles.EnsureBuilt();
        BuildPlatform(EditorUserBuildSettings.activeBuildTarget, Scenes);

    }

    [MenuItem("Build/Build Android Debug")]
    public static void BuildAndroidDebug()
    {
        string oldProductName = PlayerSettings.productName;
        string oldPackageName = PlayerSettings.applicationIdentifier;
        try
        {
            PlayerSettings.productName += " Debug";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, oldPackageName + ".Debug");
            BuildPlatform(BuildTarget.Android, Scenes);
        }
        finally
        {
            PlayerSettings.productName = oldProductName;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, oldPackageName);
        }

        CreateAssetBundles.EnsureBuilt();
    }

    [MenuItem("Build/Build Android Release")]
    public static void BuildAndroidRelease()
    {
        PlayerSettings.Android.bundleVersionCode++;
        try
        {
            BuildPlatform(BuildTarget.Android, Scenes);
        }
        catch
        {
            PlayerSettings.Android.bundleVersionCode--;
            throw;
        }

        CreateAssetBundles.EnsureBuilt();
    }

    private static readonly Dictionary<BuildTarget, string[]> packageNames = Constants.packageNames;

    public static void BuildPlatform(BuildTarget platform, string[] scenes)
    {
        var platformPath = DirUtils.PlatformBuildPath(platform);

        if (platformPath == null)
        {
            Debug.LogWarningFormat(string.Format("Do not know info for {0}", platform));
            return;
        }
        
        var previousGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var previousTarget = EditorUserBuildSettings.activeBuildTarget;

        if (!Directory.Exists(Constants.BuildPath))
            Directory.CreateDirectory(Constants.BuildPath);

        try
        {
            // DirUtils.RmStreamingBundle();
            // CreateAssetBundles.EnsureBuilt(platform, setStreamingAssets:true);

            BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                target = platform,
                scenes = scenes,
                locationPathName = platformPath,
                options = BuildOptions.None
            });
        }
        finally
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(previousGroup, previousTarget);
        }
    }

    private static string[] Scenes
    {
        get
        {
            var scenes = new List<string>();

            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled)
                    scenes.Add(scene.path);

            return scenes.ToArray();
        }
    }
}