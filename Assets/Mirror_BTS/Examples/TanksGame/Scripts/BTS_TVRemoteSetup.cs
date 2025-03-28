using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTS_TVRemoteSetup : MonoBehaviour
{
	public bool m_allowExitToHome = false;
	public bool m_allowRemoteRotation = true;
    public bool m_reportAbsoluteDpadValues = false;
    public bool m_touchesEnabled = true;

		// Start is called before the first frame update
		void Start()
    {
#if UNITY_TVOS
				UnityEngine.tvOS.Remote.allowExitToHome = m_allowExitToHome;
				UnityEngine.tvOS.Remote.allowRemoteRotation = m_allowRemoteRotation;
				UnityEngine.tvOS.Remote.reportAbsoluteDpadValues = m_reportAbsoluteDpadValues;
				UnityEngine.tvOS.Remote.touchesEnabled = m_touchesEnabled;
#endif
		}


}
