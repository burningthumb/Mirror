using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burningthumb.NativeWebview
{
	public class TV_Type
	{
		private static bool? isAmazonFireTV = null;
		private static bool? isAndroidTV = null;

#if UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaClass androidUnityActivity = null;

	public static AndroidJavaObject GetUnityActivity()
	{
		if (androidUnityActivity == null)
		{
			androidUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		}

		return androidUnityActivity.GetStatic<AndroidJavaObject>("currentActivity");
	}
#endif

		public static bool IsAmazonFireTV
		{
			get
			{
				if (!isAmazonFireTV.HasValue)
				{
#if UNITY_ANDROID && !UNITY_EDITOR
				try
				{
					using (AndroidJavaObject context = GetUnityActivity().Call<AndroidJavaObject>("getApplicationContext"))
					using (AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager"))
					{
						isAmazonFireTV = packageManager.Call<bool>("hasSystemFeature", "amazon.hardware.fire_tv");
					}
				}
				catch (System.Exception e)
				{
					isAmazonFireTV = false;
				}
#else
					isAmazonFireTV = false;
#endif
				}

				return (bool)isAmazonFireTV;
			}
		}

		public static bool IsAndroidTV
		{
			get
			{
				if (!isAndroidTV.HasValue)
				{
#if UNITY_ANDROID && !UNITY_EDITOR
				try
				{
					using (AndroidJavaObject context = GetUnityActivity().Call<AndroidJavaObject>("getApplicationContext"))
					using (AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager"))
					{
						isAndroidTV = packageManager.Call<bool>("hasSystemFeature", "android.software.leanback") ||
							      packageManager.Call<bool>("hasSystemFeature", "android.hardware.type.television");
					}
				}
				catch (System.Exception e)
				{
					isAndroidTV = false;
				}
#else
					isAndroidTV = false;
#endif
				}

				return (bool)isAndroidTV;
			}
		}
	}
}
