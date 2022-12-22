using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelExit : MonoBehaviour
{
   public void ButtonYes ()
	{
		SceneManager.LoadScene(0);
	}
}
