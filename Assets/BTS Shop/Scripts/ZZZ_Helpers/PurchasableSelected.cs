using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.
     
public class PurchasableSelected : MonoBehaviour, ISelectHandler, IDeselectHandler// required interface for OnSelect
{
    public StringReference m_keySR;

    //Do this when the selectable UI object is selected.
    public void OnSelect(BaseEventData eventData)
    {
        m_keySR.Value = gameObject.name;
    }

        public void OnDeselect(BaseEventData eventData)
    {
        m_keySR.Value = "kNoSelection";
    }
}

