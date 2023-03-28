using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelExit : MonoBehaviour
{
   public void ButtonYes ()
	{
		NetworkManagerUI l_nmui = FindObjectOfType<NetworkManagerUI>();

		if (null != l_nmui)
        {
			l_nmui.ButtonStop();
        }

		SceneManager.LoadScene(0);
	}
}
