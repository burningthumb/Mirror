using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    [SerializeField] SOString m_appKey;

    public string AppKey
    {
        get
        {
            return m_appKey.Value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
 
        IronSource.Agent.init(AppKey);

        LoadRV();
    }

    void LoadRV()
    {
        IronSource.Agent.loadRewardedVideo();
    }

    public void ShowRV()
    {
        IronSource.Agent.showRewardedVideo();
    }
}
