using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    [SerializeField] SOString m_appKey;
    [SerializeField] Button m_playButton;

    [SerializeField] UnityEvent m_addPlayed;

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
        m_playButton.interactable = false;

        IronSource.Agent.init(AppKey);

        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;


        LoadRV();
    }

    public void Update()
    {
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            if (!m_playButton.interactable)
            {
                m_playButton.interactable = true;
            }
        }
    }

    //Invoked when the RewardedVideo ad view has opened.
    //Your Activity will lose focus. Please avoid performing heavy 
    //tasks till the video ad will be closed.
    void RewardedVideoAdOpenedEvent()
    {
    }

    //Invoked when the RewardedVideo ad view is about to be closed.
    //Your activity will now regain its focus.
    void RewardedVideoAdClosedEvent()
    {
    }

    //Invoked when there is a change in the ad availability status.
    //@param - available - value will change to true when rewarded videos are available. 
    //You can then show the video by calling showRewardedVideo().
    //Value will change to false when no videos are available.
    void RewardedVideoAvailabilityChangedEvent(bool a_available)
    {
        //Change the in-app 'Traffic Driver' state according to availability.
        if (a_available)
        {
            m_playButton.interactable = true;
        }
        else
        {
            m_playButton.interactable = false;
            LoadRV();
        }
    }

    //Invoked when the user completed the video and should be rewarded. 
    //If using server-to-server callbacks you may ignore this events and wait for 
    // the callback from the ironSource server.
    //@param - placement - placement object which contains the reward data
    void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
    {
        m_addPlayed.Invoke();
    }

    //Invoked when the Rewarded Video failed to show
    //@param description - string - contains information about the failure.
    void RewardedVideoAdShowFailedEvent(IronSourceError error)
    {
        LoadRV();
    }

    // ----------------------------------------------------------------------------------------
    // Note: the events below are not available for all supported rewarded video ad networks. 
    // Check which events are available per ad network you choose to include in your build. 
    // We recommend only using events which register to ALL ad networks you include in your build. 
    // ----------------------------------------------------------------------------------------

    //Invoked when the video ad starts playing. 
    void RewardedVideoAdStartedEvent()
    {
    }

    //Invoked when the video ad finishes playing. 
    void RewardedVideoAdEndedEvent()
    {
    }

    //Invoked when the video ad is clicked. 
    void RewardedVideoAdClickedEvent(IronSourcePlacement placement)
    {
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
