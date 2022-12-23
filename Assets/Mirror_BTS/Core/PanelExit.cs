using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelExit : MonoBehaviour
{
   public void ButtonYes ()
	{
		SceneManager.LoadScene(0);
	}
}
