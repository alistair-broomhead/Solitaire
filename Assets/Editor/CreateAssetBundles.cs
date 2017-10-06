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
        BuildTarget.WebGL,
        BuildTarget.StandaloneWindows64,
        BuildTarget.StandaloneOSXIntel64,
        BuildTarget.StandaloneLinux64,
    };
}

public static class DirUtils
{
    public static void RmPlatform()
    {
        RmPlatform(EditorUserBuildSettings.activeBuildTarget);
    }
    public static void RmPlatform(BuildTarget platform)
    {
        Rm(Platform(platform));
    }
    public static void Rm(string dirName)
    {
        var info = new DirectoryInfo(dirName);

        if (info.Exists)
            info.Delete(true);
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
    public static string Platform()
    {
        return Platform(EditorUserBuildSettings.activeBuildTarget);
    }
    public static string Platform(BuildTarget platform)
    {
        return Path.Combine(Constants.assetBundlePath, platform.ToString());
    }
}

public class CreateAssetBundles
{
    
    [MenuItem("Build/AssetBundles")]
    static void BuildAllAssetBundles()
    {
        DirUtils.RmPlatform();

        DirUtils.Rm(Application.streamingAssetsPath);
        
        foreach (var platform in Constants.platforms)
        {
            DirUtils.RmPlatform(platform);
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
        string directory = DirUtils.Platform(platform);

        if (!Directory.Exists(directory))
            BuildPlatform(platform, directory);

        if (setStreamingAssets)
            DirUtils.Cp(directory, Application.streamingAssetsPath);
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
        DirUtils.Rm(Constants.BuildPath);

        foreach (var platform in Constants.platforms)
            BuildPlatform(platform, scenes);
        
        CreateAssetBundles.EnsureBuilt();
    }

    [MenuItem("Build/Rebuild")]
    public static void BuildPlatform()
    {
        BuildPlatform(EditorUserBuildSettings.activeBuildTarget, Scenes);

        CreateAssetBundles.EnsureBuilt();
    }

    private static readonly Dictionary<BuildTarget, string[]> packageNames = new Dictionary<BuildTarget, string[]>
    {
        {BuildTarget.Android, new string[]{"Android", "Solitaire.apk"}},
        {BuildTarget.WebGL, new string[]{"WebGL", "Solitaire"}},
        {BuildTarget.StandaloneWindows64, new string[]{ "Win64", "Solitaire.exe"}},
        {BuildTarget.StandaloneOSXIntel64, new string[]{ "OSX64", "Solitaire.app"}},
        {BuildTarget.StandaloneLinux64, new string[]{ "Linux64", "Solitaire"}}, 
    };

    public static void BuildPlatform(BuildTarget platform, string[] scenes)
    {
        if (!packageNames.ContainsKey(platform))
        {
            Debug.LogWarningFormat(string.Format("Do not know info for {0}", platform));
            return;
        }

        string packageDir = packageNames[platform][0];
        string packageName = packageNames[platform][1];
        
        var previousGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var previousTarget = EditorUserBuildSettings.activeBuildTarget;

        if (!Directory.Exists(Constants.BuildPath))
            Directory.CreateDirectory(Constants.BuildPath);

        var platformDir = Path.Combine(Constants.BuildPath, packageDir);
        DirUtils.Rm(platformDir);

        var platformPath = Path.Combine(platformDir, packageName);
        try
        {
            CreateAssetBundles.EnsureBuilt(platform, setStreamingAssets:true);
            
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