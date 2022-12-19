using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFPS : MonoBehaviour
{
	[SerializeField] int m_vSyncCount = 1;
	[SerializeField] int m_targetFrameRate = 30;

	// Start is called before the first frame update
	void Start()
	{

		QualitySettings.vSyncCount = m_vSyncCount;
		Application.targetFrameRate = m_targetFrameRate;

	}
}
