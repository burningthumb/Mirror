using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.burningthumb
{
	public class BTSShopProtocol : MonoBehaviour
	{
		public static string kProtocolVersion = "2";
		public static string ReqNewUser = "ReqNewUser";
		public static string ReqDeleteUser = "ReqDeleteUser";
		public static string ReqConnect = "ReqConnect";
		public static string ReqSetUGKV = "ReqSetUGKV";

		[Serializable]
		public class BTSKVP
		{
			public string key;
			public string value;
		}

		[Serializable]
		public class RequestNewUser
		{
			public string protocolVersion;
			public string apiKey;
			public string requestCode;
			public string existingAccount;
		}

		[Serializable]
		public class ResponseNewUser
		{
			public string protocolVersion;
			public string apiKey;
			public string responseCode;
			public string account;
			public string temppassword;
			public string result;
		}

		[Serializable]
		public class RequestDeleteUser
		{
			public string protocolVersion;
			public string apiKey;
			public string requestCode;
			public string account;
		}

		[Serializable]
		public class ResponseDeleteUser
		{
			public string protocolVersion;
			public string apiKey;
			public string responseCode;
			public string account;
			public string result;
		}

		[Serializable]
		public class RequestConnect
		{
			public string protocolVersion;
			public string apiKey;
			public string requestCode;
			public string account;
			public string password;
			public string temppassword;
			public string game;
			public List<BTSKVP> keyValuePair;
		}

		[Serializable]
		public class ResponseConnect
		{
			public string protocolVersion;
			public string apiKey;
			public string responseCode;
			public string result;
			public List<BTSKVP> keyValuePair;
		}

		[Serializable]
		public class RequestSetUGKV
		{
			public string protocolVersion;
			public string apiKey;
			public string requestCode;
			public string account;
			public string password;
			public string game;
			public string key;
			public string value;
		}

		[Serializable]
		public class ResponseSetUGKV
		{
			public string protocolVersion;
			public string apiKey;
			public string responseCode;
			public string result;
		}


	}
}
