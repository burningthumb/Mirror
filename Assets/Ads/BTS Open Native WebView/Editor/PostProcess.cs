using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostProcess
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string target = proj.GetUnityFrameworkTargetGuid();
        proj.AddFrameworkToProject(target, "WebKit.framework", false);
        UnityEngine.Debug.Log("Added WebKit.framework to UnityFramework for iOS");

        proj.WriteToFile(projPath);
    }
}