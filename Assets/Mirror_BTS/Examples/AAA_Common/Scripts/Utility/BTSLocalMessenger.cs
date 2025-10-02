using UnityEngine;
using System.Collections;

public class BTSLocalMessenger : MonoBehaviour {

private static Hashtable g_messages;
private static BTSLocalMessenger g_sharedInstance;
	
	private static Hashtable messages
	{
		get 
		{
			if (null == g_messages)
			{
				g_messages = new Hashtable();
			}
			
			return g_messages;
		}
	}

	public static BTSLocalMessenger sharedInstance
    {
        get 
		{ 
			if (null == g_sharedInstance)
			{
				string l_className = typeof(BTSLocalMessenger).Name;
				GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
				g_sharedInstance = l_go.AddComponent<BTSLocalMessenger>() as BTSLocalMessenger;

				DontDestroyOnLoad(g_sharedInstance);
			}

			return g_sharedInstance;
		}
    }

	static public void RegisterFor(GameObject a_gameObject, string a_message)
	{

		ArrayList l_listeners;
		
		if (BTSLocalMessenger.messages.Contains(a_message))
		{
			l_listeners = BTSLocalMessenger.messages[a_message] as ArrayList;
		}
		else
		{
			l_listeners = new ArrayList();
			BTSLocalMessenger.messages.Add(a_message, l_listeners);
		}
		
		if (l_listeners.Contains(a_gameObject))
		{
			return;
		}
		
		l_listeners.Add(a_gameObject);
		
	}
	
	static public void Send(string a_message)
	{
		BTSLocalMessenger.Send(a_message, new Hashtable());
	}

	static public void Send(string a_message, Hashtable l_params)
	{
		//Debug.Log("Send(string a_message, Hashtable l_params): " + a_message + " / " + l_params.Count);
		ArrayList l_listeners;
		ArrayList l_cleanup = new ArrayList();
		
		if (BTSLocalMessenger.messages.Contains(a_message))
		{
			l_listeners = BTSLocalMessenger.messages[a_message] as ArrayList;
			
			foreach (GameObject l_gameObject in l_listeners)
			{
				if (l_gameObject)
				{
//					Debug.Log (l_gameObject + " " + a_message);
					l_gameObject.SendMessage(a_message, l_params, SendMessageOptions.DontRequireReceiver);
					l_cleanup.Add(l_gameObject);
				}
			}
			
			l_listeners = l_cleanup;
		}
	}
		
public static void SendNotification(string a_notification)
        {
				Debug.LogWarning("BTSLocalMessenger.SendNotification is depricated.  Use BTSLocalMessenger.Send instead.");
                BTSLocalMessenger.Send(a_notification, null);
        }

}
