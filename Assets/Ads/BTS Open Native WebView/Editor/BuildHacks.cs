using UnityEditor;
using UnityEngine;

    public class BuildHacks {

        /// <summary>
        /// https://forum.unity.com/threads/android-builds-failing-when-script-debugging-is-enabled.1027357/
        /// Android builds failing when 'Script Debugging' is enabled
        /// </summary>
        [InitializeOnLoadMethod]
        static void FixScriptDebuggingBuildFail()
        {
    #if UNITY_STANDALONE_OSX
            Debug.Log("[OS X Build Patch] Linker flags");
            PlayerSettings.SetAdditionalIl2CppArgs("--linker-flags=\"-framework Cocoa -framework WebKit\"");
    #endif

    #if UNITY_ANDROID
            Debug.Log("[Android Build Patch] Linker flags");
            PlayerSettings.SetAdditionalIl2CppArgs("");
    #endif
        }
    }