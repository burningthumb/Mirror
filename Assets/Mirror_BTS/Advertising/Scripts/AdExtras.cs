using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdExtras : MonoBehaviour
{

    public void QueueAd()
    {
        
        PlayerPrefs.SetInt(UnityAdManager.keys.AdDiceRoll.ToString(), Random.Range(1,7));
        PlayerPrefs.Save();
    }
}
