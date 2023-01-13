using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class ShowIPAddress : MonoBehaviour
{
    TMP_Text m_ipText;

    // Start is called before the first frame update
    void Start()
    {
        if (null == m_ipText)
        {
            m_ipText = GetComponent<TMP_Text>();
        }

        if (null != m_ipText)
        {
            m_ipText.text = "IP: " + GetIPAddress();
        }
    }

    string GetIPAddress()
    { 
         var host = Dns.GetHostEntry(Dns.GetHostName());

         foreach (var ip in host.AddressList)
         {
             if (ip.AddressFamily == AddressFamily.InterNetwork)
             {
                 return ip.ToString();
             }
         }

        return "No Network";
    }
}
