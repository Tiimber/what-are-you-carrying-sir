#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class IncrementVersionNumber
{
    [MenuItem("Exoa/Build/Increment version")]
    public static void IncrementVersion()
    {
        string version = PlayerSettings.bundleVersion;
        string[] parts = version.Split('.');
        int lastNum = int.Parse(parts[parts.Length - 1]);
        lastNum++;
        string newVersion = "";
        for (int i = 0; i < parts.Length - 1; i++)
            newVersion += parts[i] + ".";
        newVersion += lastNum;
        int newVersionCode = int.Parse(newVersion.Replace(".", ""));
        Debug.Log("IncrementVersion " + version + " " + newVersion + " " + newVersionCode);
        PlayerSettings.bundleVersion = newVersion;
        PlayerSettings.Android.bundleVersionCode = newVersionCode;
    }

    [PostProcessBuild(1080)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        Debug.Log("OnPostProcessBuild  " + target + " " + path);
        IncrementVersion();
    }

    //public class MyBuildPostprocessor {
    //    [PostProcessBuildAttribute(1)]
    //    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
    //        Debug.Log( pathToBuiltProject );
    //    }
    //}
}
#endif
