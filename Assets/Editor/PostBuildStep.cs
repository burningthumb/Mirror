using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#if UNITY_IOS
//using UnityEditor.iOS.Xcode;
#endif
using System.IO;

using UnityEngine;

public class PostBuildStep
{
#if UNITY_IOS
	private const string CodeSignEntitlementsPropertyName = "CODE_SIGN_ENTITLEMENTS";
	private const string EntitlementsPlistFilenameExtension = ".entitlements";
	private const string NetworkingMulticastEntitlementKey = "com.apple.developer.networking.multicast";
	private const string GameCenterEntitlementKey = "com.apple.developer.game-center";
	private const bool NetworkingMulticastEntitlementValue = true;
	private const bool GameCenterEntitlementValue = true;
#endif // UNITY_IOS

	// Set the IDFA request description:
	const string k_TrackingDescription = "Your data will be used to provide you a better and personalized ad experience.";
	const string k_LocalNetworkUsageDescription = "This game can find other copies running on your local area network when you allow local network usage.";
	[PostProcessBuild(0)]
	public static void OnPostProcessBuild0(BuildTarget buildTarget, string pathToXcode)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			AddPListValues(pathToXcode);
		}
	}

	// Implement a function to read and write values to the plist file:
	static void AddPListValues(string pathToXcode)
	{
		// Retrieve the plist file from the Xcode project directory:
		string plistPath = pathToXcode + "/Info.plist";

		PlistDocument plistObj = new PlistDocument();

		// Read the values from the plist file:
		plistObj.ReadFromString(File.ReadAllText(plistPath));

		// Set values from the root object:
		PlistElementDict plistRoot = plistObj.root;

		// Set the description key-value in the plist:
		plistRoot.SetString("NSUserTrackingUsageDescription", k_TrackingDescription);
		plistRoot.SetString("NSLocalNetworkUsageDescription", k_LocalNetworkUsageDescription);
		plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);

		// Save changes to the plist:
		File.WriteAllText(plistPath, plistObj.WriteToString());

	}

	[PostProcessBuild(2)]
	public static void OnPostProcessBuild2(BuildTarget target, string pathToBuiltProject)
	{
		switch (target)
		{
			case BuildTarget.iOS:
				PostProcessEntitlements(pathToBuiltProject);
				break;
			default:
				// nothing to do for this platform.
				return;
		}
	}

	private static void PostProcessEntitlements(string pathToBuiltProject)
	{
#if UNITY_IOS
		// load project
		string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
		var project = new PBXProject();

		Debug.Log($"projectPath = {projectPath}");

		project.ReadFromFile(projectPath);

		string targetGuid = project.GetUnityMainTargetGuid();

		Debug.Log($"targetGuid = {targetGuid}");

//		project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

		// create or modify the entitlements plist
		PlistDocument plist = new PlistDocument();

		string plistFilePath = project.GetEntitlementFilePathForTarget(targetGuid);
		string plistFileName;
		bool addEntitlementFile = false;

		Debug.Log($"plistFilePath = {plistFilePath}");

		Debug.Log($"{string.IsNullOrEmpty(plistFilePath)} {plistFilePath}");

		// if we don't have an entitlements file already...
		if (string.IsNullOrEmpty(plistFilePath))
		{
			// ...get a path for a to create a new one.
			plistFileName = $"{Application.productName}{EntitlementsPlistFilenameExtension}";
			plistFilePath = Path.Combine(pathToBuiltProject, plistFileName);
			addEntitlementFile = true;
			plist.Create();
		}
		else
		{
			if (!File.Exists(plistFilePath))
			{ 
				plist.Create();
				plist.WriteToFile(plistFilePath);
				Debug.Log($"Strange bug, had to create Entitlements plist at {plistFilePath}");
			}
			// ...just snag the basename from the path.
			plistFileName = Path.GetFileName(plistFilePath);
			plist.ReadFromFile(plistFilePath);
		}

		// modify the plist
		PlistElementDict root = plist.root;
		root.SetBoolean(NetworkingMulticastEntitlementKey, NetworkingMulticastEntitlementValue);
		root.SetBoolean(GameCenterEntitlementKey, GameCenterEntitlementValue);

		// save the modified plist
		plist.WriteToFile(plistFilePath);
		Debug.Log($"Wrote Entitlements plist to {plistFilePath}");

		if (addEntitlementFile)
		{
			// add entitlements plist to project
			project.AddFile(plistFilePath, plistFileName);
			project.AddBuildProperty(targetGuid, CodeSignEntitlementsPropertyName, plistFilePath);
			project.WriteToFile(projectPath);
			Debug.Log($"Added Entitlements plist to project (target: {targetGuid})");
		}
#endif // UNITY_IOS
	}

	[PostProcessBuild(999)]
	public static void OnPostProcessBuild999(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			ModifyFrameworks(path);
		}
	}

	private static void ModifyFrameworks(string path)
	{
		string projPath = PBXProject.GetPBXProjectPath(path);

		var project = new PBXProject();
		project.ReadFromFile(projPath);

		string mainTargetGuid = project.GetUnityMainTargetGuid();

		foreach (var targetGuid in new[] { mainTargetGuid, project.GetUnityFrameworkTargetGuid() })
		{
			project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
		}

		project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

		project.WriteToFile(projPath);
	}
}