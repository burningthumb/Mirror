using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResetGlobals : MonoBehaviour
{
    public FloatReference m_CurrentSpeed;
    public FloatReference m_EffectsVolume;
    public FloatReference m_MusicVolume;

    public BoolReference m_RaceWasStopped;

    public StringReference m_SelectedLevel;

    public BoolReference m_ShowMenu;
    public IntReference m_ShowMenuIndex;

     public UnityEvent m_EventsToSend;

    // Start is called before the first frame update
    void Start()
    {
        m_CurrentSpeed.Variable.Value = 0f;
        m_EffectsVolume.Variable.Value = 0.5f;
        m_MusicVolume.Variable.Value = 0.5f;

        m_RaceWasStopped.Variable.Value = false;

        m_SelectedLevel.Variable.Value = "Practice";

        m_ShowMenu.Variable.Value = true;
        m_ShowMenuIndex.Variable.Value = 0;

	m_EventsToSend.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
