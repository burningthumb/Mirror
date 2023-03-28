using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror.Discovery;

public class NetworkManagerUI : MonoBehaviour
{
	[SerializeField] bool m_hideNetworkPanelOnClick = true;
	[SerializeField] bool m_advertiseServer = true;

	[SerializeField] Button m_buttonHide;

	[SerializeField] GameObject PanelStart;
	[SerializeField] GameObject PanelStop;
	[SerializeField] GameObject PanelFind;

	[SerializeField] GameObject ScrollViewContent;

	[SerializeField] Button buttonHost, buttonServer, buttonClient, buttonStop, buttonFind;

	[SerializeField] TMP_InputField inputFieldAddress;

	[SerializeField] TMP_Text serverText;
	[SerializeField] TMP_Text clientText;

	// Mirror Network Discovery
	readonly Dictionary<string, ServerResponse> discoveredServers = new Dictionary<string, ServerResponse>();
	public NetworkDiscovery networkDiscovery;

#if UNITY_EDITOR
	void OnValidate()
	{
		if (networkDiscovery == null)
		{
			networkDiscovery = GetComponent<NetworkDiscovery>();
			UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
			UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
		}
	}
#endif

	public void Start()
	{
		//Update the canvas text if you have manually changed network managers address from the game object before starting the game scene
		if (NetworkManager.singleton.networkAddress != "localhost") { inputFieldAddress.text = NetworkManager.singleton.networkAddress; }

		//Adds a listener to the main input field and invokes a method when the value changes.
		inputFieldAddress.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

		//Make sure to attach these Buttons in the Inspector
		buttonHost.onClick.AddListener(ButtonHost);
		buttonServer.onClick.AddListener(ButtonServer);
		buttonClient.onClick.AddListener(ButtonClient);
		buttonStop.onClick.AddListener(ButtonStop);
		buttonFind.onClick.AddListener(ButtonFind);

		//This updates the Unity canvas, we have to manually call it every change, unlike legacy OnGUI.
		SetupCanvas();
	}

	// Invoked when the value of the text field changes.
	public void ValueChangeCheck()
	{
		NetworkManager.singleton.networkAddress = inputFieldAddress.text;
	}

	public void ButtonHost()
	{
		if (m_advertiseServer)
		{
			discoveredServers.Clear();
		}

		NetworkManager.singleton.StartHost();

		if (m_advertiseServer)
		{
			networkDiscovery.AdvertiseServer();
		}

		SetupCanvas();

		if (null != m_buttonHide && m_hideNetworkPanelOnClick)
		{
			m_buttonHide.onClick.Invoke();
		}
	}

	public void ButtonServer()
	{
		if (m_advertiseServer)
		{
			discoveredServers.Clear();
		}

		NetworkManager.singleton.StartServer();

		if (m_advertiseServer)
		{
			networkDiscovery.AdvertiseServer();
		}

		SetupCanvas();

		if (null != m_buttonHide && m_hideNetworkPanelOnClick)
		{
			m_buttonHide.onClick.Invoke();
		}
	}

	public void ButtonClient()
	{
		StopDiscoveryAndClearScrollView();

		NetworkManagerDelegates.onClientConnectDelegate += OnClientConnectDelegate;
		NetworkManagerDelegates.onClientDisconnectDelegate += OnClientDisconnectDelegate;

		NetworkManager.singleton.StartClient();

		SetupCanvas();

		if (null != m_buttonHide && m_hideNetworkPanelOnClick)
		{
			m_buttonHide.onClick.Invoke();
		}
	}

	public void ButtonStop()
	{
		// stop host if host mode
		if (NetworkServer.active && NetworkClient.isConnected)
		{
			NetworkManager.singleton.StopHost();

			if (m_advertiseServer)
			{
				networkDiscovery.AdvertiseServer();
			}
		}
		// stop client if client-only
		else if (NetworkClient.isConnected)
		{
			NetworkManager.singleton.StopClient();
		}
		// stop server if server-only
		else if (NetworkServer.active)
		{
			NetworkManager.singleton.StopServer();

			if (m_advertiseServer)
			{
				networkDiscovery.AdvertiseServer();
			}
		}

		SetupCanvas();

		if (null != m_buttonHide && m_hideNetworkPanelOnClick)
		{
			m_buttonHide.onClick.Invoke();
		}
	}

	public void SetupCanvas()
	{
		// Here we will dump majority of the canvas UI that may be changed.

		if (!NetworkClient.isConnected && !NetworkServer.active)
		{
			if (NetworkClient.active)
			{
				PanelStart.SetActive(false);
				PanelStop.SetActive(true);
				clientText.text = "Connecting to " + NetworkManager.singleton.networkAddress + " ...";
			}
			else
			{
				PanelStart.SetActive(true);
				PanelStop.SetActive(false);
			}
		}
		else
		{
			PanelStart.SetActive(false);
			PanelStop.SetActive(true);

			// server / client status message
			if (NetworkServer.active)
			{
				serverText.text = "Server: " + ShowIPAddress.GetIPAddress() + " Transport: " + Transport.active;
			}
			if (NetworkClient.isConnected)
			{
				clientText.text = "Client: Address: " + ShowIPAddress.GetIPAddress(); // NetworkManager.singleton.networkAddress;
			}
		}

	}

	// Mirror Network Discovery

	public void ButtonFind()
	{
		StopDiscoveryAndClearScrollView();

		networkDiscovery.StartDiscovery();
	}

	private void StopDiscoveryAndClearScrollView()
	{
		networkDiscovery.StopDiscovery();

		ClearScrollView();
	}

	private void ClearScrollView()
	{
		discoveredServers.Clear();

		foreach (Button l_button in ScrollViewContent.transform.GetComponentsInChildren<Button>())
		{
			Destroy(l_button.gameObject);
		}
	}

	public void ButtonJoin(ServerResponse a_info)
	{
		StopDiscoveryAndClearScrollView();

		NetworkManagerDelegates.onClientConnectDelegate += OnClientConnectDelegate;
		NetworkManagerDelegates.onClientDisconnectDelegate += OnClientDisconnectDelegate;

		NetworkManager.singleton.StartClient(a_info.uri);

		SetupCanvas();

		if (null != m_buttonHide && m_hideNetworkPanelOnClick)
		{
			m_buttonHide.onClick.Invoke();
		}
	}

	void OnClientConnectDelegate()
	{
		Debug.Log("OnClientConnectDelegate");

		SetupCanvas();
	}

	void OnClientDisconnectDelegate()
	{
		Debug.Log("OnClientDisconnectDelegate");

		if (NetworkClient.isConnected)
		{
			NetworkManager.singleton.StopClient();
		}

		//		StopDiscoveryAndClearScrollView();

		SetupCanvas();

		NetworkManagerDelegates.onClientConnectDelegate -= OnClientConnectDelegate;
		NetworkManagerDelegates.onClientDisconnectDelegate -= OnClientDisconnectDelegate;

	}

	public void OnDiscoveredServer(ServerResponse info)
	{
		// Ignore re-discovered severs by server id
		if (discoveredServers.ContainsKey(info.serverId.ToString()))
		{
			return;
		}

		// Ignore re-discovered severs by server address
		foreach (ServerResponse l_info in discoveredServers.Values)
		{
			if (l_info.EndPoint.Address == info.EndPoint.Address)
			{
				return;
			}
		}


		// Note that you can check the versioning to decide if you can connect to the server or not using this method
		discoveredServers[info.serverId.ToString()] = info;

		foreach (ServerResponse l_info in discoveredServers.Values)
		{
			TMP_DefaultControls.Resources l_resources = new TMP_DefaultControls.Resources();
			GameObject l_buttonGO = TMP_DefaultControls.CreateButton(l_resources);

			Button l_button = l_buttonGO.GetComponentInChildren<Button>();
			TMP_Text l_buttonText = l_buttonGO.GetComponentInChildren<TMP_Text>();

			l_buttonGO.transform.SetParent(ScrollViewContent.transform, false);

			l_buttonText.text = l_info.EndPoint.Address.ToString(); //  + " (" + l_info.serverId + ")"  ;

			l_button.onClick.AddListener(() => { ButtonJoin(l_info); });

		}
	}

}
