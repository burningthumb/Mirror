using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTSSelectThis : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        BTSPulseSelected.AddSelectThis(gameObject.GetComponent<Selectable>());
    }

    // Update is called once per frame
    void OnDisable()
    {
            BTSPulseSelected.RemoveSelectThis(gameObject.GetComponent<Selectable>());
    }
}
