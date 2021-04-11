using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildPlayer
{
    [MenuItem("Build/Windows (Build)")]
    public static void Build_Windows_OnlyBuild()
    {
        Build(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build/MacOS (Build)")]
    public static void Build_MacOS_OnlyBuild()
    {
        Build(BuildTarget.StandaloneOSX);
    }

    public static string GetPlatformForPackRes(BuildTarget target)
    {
        string res = "";
        switch (target)
        {
            case BuildTarget.StandaloneOSX:
            {
                res = "MacOS";
                break;
            }
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            {
                res = "Windows";
                break;
            }
        }

        return res;
    }

    private static string BuildFolder => Application.dataPath + "/../../Builds/";
    private static string ProductName = "Sokoban Alchemist";
    private static string WwiseSoundBankFolder => Application.streamingAssetsPath + "/Audio/GeneratedSoundBanks/";
    private static string WwiseSoundBankFolder_Back => Application.dataPath + "/StreamingAssets_back/Audio/GeneratedSoundBanks/";

    private static void Build(BuildTarget build_target)
    {
        string platform = GetPlatformForPackRes(build_target);
        string build_path = "";
        string build_ExecutableFile = "";
        if (platform == "Windows")
        {
            build_path = BuildFolder + platform + "/";
            build_ExecutableFile = build_path + ProductName + ".exe";
        }
        else if (platform == "MacOS")
        {
            build_path = BuildFolder + platform + "/";
            build_ExecutableFile = build_path + ProductName + ".app";
        }

        string[] levels =
        {
            "Assets/Scenes/MainScene.unity",
        };

        BuildOptions option_build = BuildOptions.CompressWithLz4;
        PlayerSettings.productName = ProductName;

        string audio_Windows = WwiseSoundBankFolder + "Windows/";
        string audio_Mac = WwiseSoundBankFolder + "Mac/";

        string audio_back = WwiseSoundBankFolder_Back;
        string audio_Windows_back = WwiseSoundBankFolder_Back + "Windows/";
        string audio_MacOS_back = WwiseSoundBankFolder_Back + "Mac/";

        if (Directory.Exists(audio_back)) Directory.Delete(audio_back, true);
        Directory.CreateDirectory(audio_back);

        if (Directory.Exists(build_path)) Directory.Delete(build_path, true);
        Directory.CreateDirectory(build_path);

        if (build_target == BuildTarget.StandaloneWindows64)
        {
            if (Directory.Exists(audio_Mac)) Directory.Move(audio_Mac, audio_MacOS_back);
        }
        else if (build_target == BuildTarget.StandaloneOSX)
        {
            if (Directory.Exists(audio_Windows)) Directory.Move(audio_Windows, audio_Windows_back);
        }

        try
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            BuildPipeline.BuildPlayer(levels, build_ExecutableFile, build_target, option_build);
        }
        catch
        {
            // ignored
        }
        finally
        {
            if (build_target == BuildTarget.StandaloneWindows64)
            {
                if (Directory.Exists(audio_Mac)) Directory.Delete(audio_Mac, true);
                Directory.Move(audio_MacOS_back, audio_Mac);
            }
            else if (build_target == BuildTarget.StandaloneOSX)
            {
                if (Directory.Exists(audio_Windows)) Directory.Delete(audio_Windows, true);
                Directory.Move(audio_Windows_back, audio_Windows);
            }
        }
    }
}