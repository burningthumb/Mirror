using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace com.burningthumb
{
	public class BTSShop : MonoBehaviour
	{

		private static BTSShop m_instance;

		public static BTSShop SharedInstance
		{
			get
			{
				return m_instance;
			}

			set
			{
				m_instance = value;
			}
		}

		public bool m_clearShopData = false;

		public bool m_dontDestroyOnLoad = true;

		public bool m_ShopIsOpen = true;

		public Canvas m_buttonCanvas;
		public Canvas m_shopCanvas;

		public string m_GameIdSuffix = "btskart";

		private class BTSShopItem : Object
		{
			private string m_ownedValue;
			private int m_cost;
			private string m_uiName;
			private string m_uiDescription;
			private Sprite m_uiSprite;

			public string OwnedValue
			{
				get { return m_ownedValue; }
				set { m_ownedValue = value; }
			}

			public int Cost
			{
				get { return m_cost; }
				set { m_cost = value; }
			}

			public string UIName
			{
				get { return m_uiName; }
				set { m_uiName = value; }
			}

			public string UIDescription
			{
				get { return m_uiDescription; }
				set { m_uiDescription = value; }
			}

			public Sprite UISprite
			{
				get { return m_uiSprite; }
				set { m_uiSprite = value; }
			}

		}

		public enum ShopKey { kOwnedValue, kCost, kUIName, kUIDescripton, kUISprite }

		public string m_API_Key = "Invalid API Key";
		public string m_hostURL = "http://localhost/~robert/btsshop/";
		public bool m_useTestHost = false;
		public string m_testHostURL = "http://localhost/~robert/btsshop/";
		public string m_newuserScript = "newuser.php";
		public string m_deleteuserScript = "deleteuser.php";
		public string m_connectScript = "connect.php";
		public string m_coinScript = "coin.php";
		public string m_setValueScript = "setugkv.php";

		public delegate void OnAccountFound();
		public static event OnAccountFound onAccountFound;

		public delegate void OnAccountNotFound();
		public static event OnAccountNotFound onAccountNotFound;

		public delegate void OnNewAccount();
		public static event OnNewAccount onNewAccount;

		public delegate void OnNewAccountError();
		public static event OnNewAccountError onNewAccountError;

		public delegate void OnDeleteAccount();
		public static event OnDeleteAccount onDeleteAccount;

		public delegate void OnDeleteAccountError();
		public static event OnDeleteAccountError onDeleteAccountError;

		public delegate void OnAccountInvalid();
		public static event OnAccountInvalid onAccountInvalid;

		public delegate void OnNetworkError();
		public static event OnNetworkError onNetworkError;

		public delegate void OnCoinSet();
		public static event OnCoinSet onCoinSet;

		public bool m_didValidateUser = false;

		public ColorBlock m_shopColors;
		public Color m_textColor;

		public string m_noSelectionKey = "kNoSelection";
		public string m_noSelectionUIName = "Select";
		public string m_noSelectionUIDescription = "Select an item from the list to purchase it";
		public Sprite m_noSelectionUISprite;

		public BTSShopItemV1Control[] m_btsShopItemV1Control;

		//public string [] m_key;
		//public string [] m_ownedValue;
		//public int [] m_cost;
		//public string [] m_uiName;
		//public string [] m_uiDescription;
		//public Sprite [] m_uiSprite;

		public UnityEvent m_accountFound;
		public UnityEvent m_accountNotFound;
		public UnityEvent m_accountInvalid;

		public UnityEvent m_newAccountCreated;
		public UnityEvent m_newAccountNotCreated;

		public UnityEvent m_accountDeleted;
		public UnityEvent m_accountNotDeleted;

		public UnityEvent m_networkError;

		private Dictionary<string, BTSShopItem> m_dictionary;
		private int m_totalStoreValue = 0;
		private static string m_lastNetworkError;

		public static void RaiseOnAccountFound(UnityEvent a_unityEvent)
		{
			onAccountFound?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnAccountNotFound(UnityEvent a_unityEvent)
		{
			onAccountNotFound?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnNewAccount(UnityEvent a_unityEvent)
		{
			onNewAccount?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnNewAccountError(UnityEvent a_unityEvent)
		{
			onNewAccountError?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnDeleteAccount(UnityEvent a_unityEvent)
		{
			onDeleteAccount?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnDeleteAccountError(UnityEvent a_unityEvent)
		{
			onDeleteAccountError?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnAccountInvalid(UnityEvent a_unityEvent)
		{
			onAccountInvalid?.Invoke();
			a_unityEvent.Invoke();
		}

		public static void RaiseOnNetworkError(UnityEvent a_unityEvent)
		{
			onNetworkError?.Invoke();
			a_unityEvent.Invoke();
		}

		public static string LastNetworkError
		{
			get
			{
				return m_lastNetworkError;
			}

			set
			{
				m_lastNetworkError = value;
			}
		}

		public string OwnedValue(string a_key)
		{
			SetupDictionaryIfNeeded();
			if (m_dictionary.ContainsKey(a_key))
			{
				return m_dictionary[a_key].OwnedValue;
			}

			return "kUnknown";
		}

		public int Cost(string a_key)
		{
			SetupDictionaryIfNeeded();
			if (m_dictionary.ContainsKey(a_key))
			{
				return m_dictionary[a_key].Cost;
			}

			return 0;
		}

		public string UIName(string a_key)
		{
			SetupDictionaryIfNeeded();

			if (m_dictionary.ContainsKey(a_key))
			{
				return m_dictionary[a_key].UIName;
			}

			return m_noSelectionUIName;
		}

		public string UIDescription(string a_key)
		{
			SetupDictionaryIfNeeded();

			if (m_dictionary.ContainsKey(a_key))
			{
				return m_dictionary[a_key].UIDescription;
			}

			return m_noSelectionUIDescription;
		}

		public Sprite UISprite(string a_key)
		{
			SetupDictionaryIfNeeded();

			if (m_dictionary.ContainsKey(a_key))
			{
				return m_dictionary[a_key].UISprite;
			}

			return m_noSelectionUISprite;
		}

		public bool IsOwned(string a_key)
		{

			if (!SecurePlayerPrefs.isInitialized())
			{
				SecurePlayerPrefs.Init();
			}

			// Testing
			// SecurePlayerPrefs.DeleteKey(a_key);

			if (0 == Cost(a_key))
			{
				SecurePlayerPrefs.SetString(a_key, OwnedValue(a_key));
			}

			string l_value = SecurePlayerPrefs.GetString(a_key);

			if (m_dictionary.ContainsKey(a_key))
			{
				return (l_value == m_dictionary[a_key].OwnedValue);
			}

			// The BTS Shop knows nothing so its not owned
			return false;
		}

		public bool Online
		{
			get { return m_didValidateUser; }
		}

		public int Coins
		{
			get
			{
				if (!SecurePlayerPrefs.isInitialized())
				{
					SecurePlayerPrefs.Init();
				}

				int l_value = SecurePlayerPrefs.GetInt(BTSShopKey.kCoins);

				return l_value;
			}

			set
			{
				if (!SecurePlayerPrefs.isInitialized())
				{
					SecurePlayerPrefs.Init();
				}

				SecurePlayerPrefs.SetInt(BTSShopKey.kCoins, value);
				SecurePlayerPrefs.Save();

				// Tell anyone who cares
				onCoinSet?.Invoke();

				// Update the server
				StartCoroutine(SetKeyValueOnServer(BTSShopKey.kCoins, "" + value));
			}
		}

		public string HostURL
		{

			get
			{
				if (m_useTestHost)
				{
					return m_testHostURL;
				}

				return m_hostURL;
			}
		}

		public ColorBlock ShopColors
		{
			get
			{
				return m_shopColors;
			}
		}

		public Color ShopTextColor
		{
			get
			{
				return m_textColor;
			}
		}

		public void Buy100Coins()
		{
			Coins = Coins + 100;
		}

		public void SetLocalAndRemoteStringValue(string a_key, string a_value)
		{
			SecurePlayerPrefs.SetString(a_key, a_value);

			// Update the server
			StartCoroutine(SetKeyValueOnServer(BTSShopKey.GetRealKey(a_key), a_value));
		}

		public void Awake()
		{
			if (m_dontDestroyOnLoad)
			{
				if (null != SharedInstance)
				{ 
					Debug.Log("BTS Shop - There can be only 1. Destroying myself", gameObject);
					Destroy(gameObject);
					return;
				}
			}

			SharedInstance = this;
			
			if (m_dontDestroyOnLoad)
			{
				DontDestroyOnLoad(SharedInstance.gameObject);
			}
		}

		// Start is called before the first frame update
		public void Start()
		{
			if (!m_ShopIsOpen)
			{
				if (m_buttonCanvas)
				{
					m_buttonCanvas.enabled = false;
					m_buttonCanvas.gameObject.SetActive(false);
				}

				if (m_shopCanvas)
				{
					m_shopCanvas.enabled = false;
					m_shopCanvas.gameObject.SetActive(false);
				}

			}

			if (m_clearShopData)
			{
				SetupDictionaryIfNeeded();
				foreach (string l_key in m_dictionary.Keys)
				{
					SetLocalAndRemoteStringValue(l_key, "");
				}

				m_dictionary.Clear();
				m_dictionary = null;
			}

			SetupDictionaryIfNeeded();
			RegisterUserIfNeeded();
		}

		void SetupDictionaryIfNeeded()
		{
			if (null != m_dictionary)
			{
				return;
			}

			m_btsShopItemV1Control = GetComponentsInChildren<BTSShopItemV1Control>();

			m_totalStoreValue = 0;

			m_dictionary = new Dictionary<string, BTSShopItem>();


			foreach (BTSShopItemV1Control l_itemControl in m_btsShopItemV1Control)
			{
				BTSShopItem l_item = new BTSShopItem();

				l_item.OwnedValue = l_itemControl.ShopItemV1.OwnedValue;
				l_item.Cost = l_itemControl.ShopItemV1.Cost;

				if (!m_ShopIsOpen)
				{
					l_item.Cost = 0;
				}

				l_item.UIName = l_itemControl.ShopItemV1.UIName;
				l_item.UIDescription = l_itemControl.ShopItemV1.UIDescription;
				l_item.UISprite = l_itemControl.ShopItemV1.UISprite;

				m_dictionary.Add(l_itemControl.ShopItemV1.Key, l_item);

				m_totalStoreValue += l_item.Cost;
			}

			//int i = 0;

			//foreach (string l_key in m_key)
			//{
			//	BTSShopItem l_item = new BTSShopItem ();
			//	l_item.OwnedValue = m_ownedValue [i];

			//	l_item.Cost = m_cost [i];

			//	if (!m_ShopIsOpen) {
			//		l_item.Cost = 0;
			//	}

			//	l_item.UIName = m_uiName [i];
			//	l_item.UIDescription = m_uiDescription [i];
			//	l_item.UISprite = m_uiSprite [i];

			//	m_dictionary.Add (l_key, l_item);

			//	m_totalStoreValue += l_item.Cost;

			//	i++;
			//}

		}

		public void NoNetwork()
		{

		}

		public void RegisterUserIfNeeded()
		{
			if (!m_ShopIsOpen)
			{
				m_didValidateUser = true;
				return;
			}

			StartCoroutine(ValidateUser());
		}

		public void RequestNewUser()
		{
			if (!m_ShopIsOpen)
			{
				m_didValidateUser = true;
				return;
			}

			StartCoroutine(NewAccount());
		}

		IEnumerator NewAccount()
		{
			LastNetworkError = "";

			BTSShopProtocol.RequestNewUser l_request = new BTSShopProtocol.RequestNewUser();
			l_request.protocolVersion = BTSShopProtocol.kProtocolVersion;
			l_request.apiKey = m_API_Key;
			l_request.requestCode = BTSShopProtocol.ReqNewUser;
			l_request.existingAccount = SecurePlayerPrefs.GetString(BTSShopKey.kAccount);

			string l_json = JsonUtility.ToJson(l_request);
			//			Debug.Log(l_json);

			string l_finalURL = HostURL + m_newuserScript;

			Debug.Log($"{l_finalURL} {l_json}");

			using (UnityWebRequest l_uwr = UnityWebRequest.Put(l_finalURL, l_json))
			{
				yield return l_uwr.SendWebRequest();

				bool l_isNetworkError = (l_uwr.result == UnityWebRequest.Result.ConnectionError);
				bool l_isHttpError = (l_uwr.result == UnityWebRequest.Result.ProtocolError);


				if (l_isNetworkError || l_isHttpError)
				{
					LastNetworkError = l_uwr.error + " [" + HostURL + "]";
					RaiseOnNetworkError(m_networkError);
				}
				else
				{
					string l_responseJSON = l_uwr.downloadHandler.text;
					//					Debug.Log(l_responseJSON);
					BTSShopProtocol.ResponseNewUser l_responseNewUser = JsonUtility.FromJson<BTSShopProtocol.ResponseNewUser>(l_responseJSON);

					if (l_responseNewUser.account.Length > 0)
					{
						SecurePlayerPrefs.SetString(BTSShopKey.kAccount, l_responseNewUser.account);
						SecurePlayerPrefs.SetString(BTSShopKey.kTempPassword, l_responseNewUser.temppassword);
						SecurePlayerPrefs.Save();
					}

					if (BTSShopKey.RespTrue == l_responseNewUser.result)
					{
						RaiseOnNewAccount(m_newAccountCreated);
					}
					else
					{
						RaiseOnNewAccountError(m_newAccountNotCreated);
					}

				}

			}


		}

		public void RequestDeleteUser()
		{
			if (!m_ShopIsOpen)
			{
				m_didValidateUser = true;
				return;
			}

			StartCoroutine(DeleteAccount());
		}

		IEnumerator DeleteAccount()
		{
			LastNetworkError = "";

			BTSShopProtocol.RequestDeleteUser l_request = new BTSShopProtocol.RequestDeleteUser();
			l_request.protocolVersion = BTSShopProtocol.kProtocolVersion;
			l_request.apiKey = m_API_Key;
			l_request.requestCode = BTSShopProtocol.ReqDeleteUser;
			l_request.account = SecurePlayerPrefs.GetString(BTSShopKey.kAccount);

			string l_json = JsonUtility.ToJson(l_request);
			Debug.Log(l_json);

			string l_finalURL = HostURL + m_deleteuserScript;

			using (UnityWebRequest l_uwr = UnityWebRequest.Put(l_finalURL, l_json))
			{
				yield return l_uwr.SendWebRequest();

				bool l_isNetworkError = (l_uwr.result == UnityWebRequest.Result.ConnectionError);
				bool l_isHttpError = (l_uwr.result == UnityWebRequest.Result.ProtocolError);


				if (l_isNetworkError || l_isHttpError)
				{
					LastNetworkError = l_uwr.error + " [" + HostURL + "]";
					RaiseOnNetworkError(m_networkError);
				}
				else
				{
					string l_responseJSON = l_uwr.downloadHandler.text;
					Debug.Log(l_responseJSON);
					BTSShopProtocol.ResponseDeleteUser l_responseDeleteUser = JsonUtility.FromJson<BTSShopProtocol.ResponseDeleteUser>(l_responseJSON);

					if ("1" == l_responseDeleteUser.result)
					{
						SecurePlayerPrefs.DeleteKey(BTSShopKey.kAccount);
						SecurePlayerPrefs.DeleteKey(BTSShopKey.kPassword);
						SecurePlayerPrefs.DeleteKey(BTSShopKey.kTempPassword);
						SecurePlayerPrefs.Save();
					}

					if (BTSShopKey.RespTrue == l_responseDeleteUser.result)
					{
						RaiseOnDeleteAccount(m_accountDeleted);
					}
					else
					{
						RaiseOnDeleteAccountError(m_accountNotDeleted);
					}

				}

			}
		}


		IEnumerator ValidateUser()
		{
			string l_account = SecurePlayerPrefs.GetString(BTSShopKey.kAccount);
			string l_password = SecurePlayerPrefs.GetString(BTSShopKey.kPassword);
			string l_temppassword = SecurePlayerPrefs.GetString(BTSShopKey.kTempPassword);

			LastNetworkError = "";

			BTSShopProtocol.RequestConnect l_request = new BTSShopProtocol.RequestConnect();
			l_request.protocolVersion = BTSShopProtocol.kProtocolVersion;
			l_request.apiKey = m_API_Key;
			l_request.requestCode = BTSShopProtocol.ReqConnect;
			l_request.account = l_account;
			l_request.password = l_password;
			l_request.temppassword = l_temppassword;
			l_request.game = GetGame();
			l_request.keyValuePair = GetKeyValuePairs();

			string l_json = JsonUtility.ToJson(l_request);
			//			Debug.Log(l_json);

			if ((l_account.Length > 6) && (l_password.Length > 5))
			{
				string l_finalURL = HostURL + m_connectScript;
				//				Debug.Log(l_finalURL);
				using (UnityWebRequest l_uwr = UnityWebRequest.Put(l_finalURL, l_json))
				{
					yield return l_uwr.SendWebRequest();

					bool l_isNetworkError = l_uwr.result == UnityWebRequest.Result.ConnectionError;
					bool l_isHTTPError = l_uwr.result == UnityWebRequest.Result.ProtocolError;

					if (l_isNetworkError || l_isHTTPError)
					{
						//						Debug.Log(l_uwr.error);
						LastNetworkError = l_uwr.error + " [" + HostURL + "]";
						RaiseOnNetworkError(m_networkError);
					}
					else
					{
						string l_responseJSON = l_uwr.downloadHandler.text;
						//						Debug.Log(l_responseJSON);
						BTSShopProtocol.ResponseConnect l_response = JsonUtility.FromJson<BTSShopProtocol.ResponseConnect>(l_responseJSON);

						if (BTSShopKey.RespTrue == l_response.result)
						{
							SecurePlayerPrefs.DeleteKey(BTSShopKey.kTempPassword);

							foreach (BTSShopProtocol.BTSKVP l_kvp in l_response.keyValuePair)
							{
								if (l_kvp.key == BTSShopKey.kCoins)
								{
									try
									{
										SecurePlayerPrefs.SetInt(l_kvp.key, int.Parse(l_kvp.value));
									}
									catch (System.Exception e)
									{
										Debug.Log(e);
									}
								}
								else
								{
									SecurePlayerPrefs.SetString(l_kvp.key, l_kvp.value);
								}

							}

							m_didValidateUser = true;
							RaiseOnAccountFound(m_accountFound);
						}
						else
						{
							m_didValidateUser = false;
							RaiseOnAccountNotFound(m_accountNotFound);
						}

					}

				}
			}
			else
			{
				SecurePlayerPrefs.DeleteKey(BTSShopKey.kAccount);
				SecurePlayerPrefs.DeleteKey(BTSShopKey.kPassword);
				SecurePlayerPrefs.DeleteKey(BTSShopKey.kTempPassword);
				m_didValidateUser = false;

				RaiseOnAccountInvalid(m_accountInvalid);
			}

		}

		IEnumerator SetKeyValueOnServer(string a_key, string a_value)
		{
			string l_account = SecurePlayerPrefs.GetString(BTSShopKey.kAccount);
			string l_password = SecurePlayerPrefs.GetString(BTSShopKey.kPassword);

			LastNetworkError = "";
			BTSShopProtocol.RequestSetUGKV l_request = new BTSShopProtocol.RequestSetUGKV();
			l_request.protocolVersion = BTSShopProtocol.kProtocolVersion;
			l_request.apiKey = m_API_Key;
			l_request.requestCode = BTSShopProtocol.ReqSetUGKV;
			l_request.account = l_account;
			l_request.password = l_password;
			l_request.game = GetGame();
			l_request.key = a_key;
			l_request.value = a_value;

			string l_json = JsonUtility.ToJson(l_request);
			//			Debug.Log(l_json);

			if ((l_account.Length > 6) && (l_password.Length > 5))
			{
				string l_finalURL = HostURL + m_setValueScript;

				using (UnityWebRequest l_uwr = UnityWebRequest.Put(l_finalURL, l_json))
				{
					yield return l_uwr.SendWebRequest();

					bool l_isNetworkError = (l_uwr.result == UnityWebRequest.Result.ConnectionError);
					bool l_isHttpError = (l_uwr.result == UnityWebRequest.Result.ProtocolError);

					if (l_isNetworkError || l_isHttpError)
					{
						//						Debug.Log(l_uwr.error);
						LastNetworkError = l_uwr.error + " [" + HostURL + "]";
						RaiseOnNetworkError(m_networkError);
					}
					else
					{
						string l_response = l_uwr.downloadHandler.text;
					}
					//					        Debug.Log(l_response);

					//	if (Keys.rTrue == l_response)
					//	{
					//		m_didValidateUser = true;
					//		m_accountFound.Invoke();
					//	}
					//	else
					//	{
					//		m_didValidateUser = false;
					//		RaiseAccountNotFound(m_accountNotFound);
					//	}

					//}

				}
			}
			else
			{
				//m_didValidateUser = false;
				//RaiseOnAccountInvalid(m_accountInvalid);
			}

		}

		public string GetGame()
		{
			string l_prefix;

			switch (Application.platform)
			{
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
					l_prefix = "mac";
					break;

				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
					l_prefix = "windows";
					break;

				case RuntimePlatform.IPhonePlayer:
					l_prefix = "ios";
					break;

				case RuntimePlatform.tvOS:
					l_prefix = "tvos";
					break;

				case RuntimePlatform.Android:
					l_prefix = "android";
					break;

				default:
					l_prefix = "mac";
					break;

			}
			return l_prefix + "." + m_GameIdSuffix;
		}

		public List<BTSShopProtocol.BTSKVP> GetKeyValuePairs()
		{
			SetupDictionaryIfNeeded();

			List<BTSShopProtocol.BTSKVP> l_keyValuePair = new List<BTSShopProtocol.BTSKVP>();

			// Coins
			if (SecurePlayerPrefs.HasKey(BTSShopKey.kCoins))
			{
				int l_coins = SecurePlayerPrefs.GetInt(BTSShopKey.kCoins);

				BTSShopProtocol.BTSKVP l_coin = new BTSShopProtocol.BTSKVP();
				l_coin.key = BTSShopKey.kCoins;
				l_coin.value = "" + l_coins;

				l_keyValuePair.Add(l_coin);
			}

			// Purchased items
			BTSShopProtocol.BTSKVP l_item;
			foreach (string l_key in m_dictionary.Keys)
			{
				string l_realKey = BTSShopKey.GetRealKey(l_key);
				if (SecurePlayerPrefs.HasKey(l_realKey))
				{
					l_item = new BTSShopProtocol.BTSKVP();
					l_item.key = l_realKey;
					l_item.value = SecurePlayerPrefs.GetString(l_realKey);
					l_keyValuePair.Add(l_item);
				}

			}

			return l_keyValuePair;
		}
	}
}
