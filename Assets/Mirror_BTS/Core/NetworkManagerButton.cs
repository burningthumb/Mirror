using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerButton : MonoBehaviour
{
	[SerializeField] Button m_button;

    // Start is called before the first frame update
    void Start()
    {
		m_button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 //   /// <summary>Like Start(), but only called on server and host.</summary>
	//public override void OnStartServer()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Stop event, only called on server and host.</summary>
	//public override void OnStopServer()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Like Start(), but only called on client and host.</summary>
	//public override void OnStartClient()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Stop event, only called on client and host.</summary>
	//public override void OnStopClient()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Like Start(), but only called on client and host for the local player object.</summary>
	//public override void OnStartLocalPlayer()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Stop event, but only called on client and host for the local player object.</summary>
	//public override void OnStopLocalPlayer()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Like Start(), but only called for objects the client has authority over.</summary>
	//public override void OnStartAuthority()
	//{
	//	m_button = GetComponent<Button>();
	//}

	///// <summary>Stop event, only called for objects the client has authority over.</summary>
	//public override void OnStopAuthority()
	//{
	//	m_button = GetComponent<Button>();
	//}
}
