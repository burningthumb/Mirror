using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.burningthumb
{
	public class BTSShopKey : MonoBehaviour
	{
		public static string kCoins = "kRandomSeed";
		public static string kAccount = "kAccount";
		public static string kEmail = "kEmail";
		public static string kPassword = "kPassword";
		public static string kTempPassword = "kTempPassword";

		public static string kGame = "kGame";
		public static string kKey = "kKey";
		public static string kValue = "kValue";

		public static string kJson = "kJson";

		public static string rOK = "OK";
		public static string rNotOK = "NOTOK";
		public static string RespTrue = "1";
		public static string RespFalse = "0";


		public static string GetRealKey(string a_key)
		{
			if (a_key == "kCoins")
			{
				return kCoins;
			}

			return a_key;

		}
	}
}
