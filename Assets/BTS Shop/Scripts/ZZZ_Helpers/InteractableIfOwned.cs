using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.burningthumb
{
	public class InteractableIfOwned : MonoBehaviour
	{
		    public StringReference m_selectedKeySR;
		    //string m_lastkeySet = null;
		    BTSShop m_BTSShop;
		    Button m_button;


		    // Start is called before the first frame update
		    void Start()
		    {
			if (null == m_BTSShop)
			{
				m_BTSShop = FindObjectOfType<BTSShop>();
			}

			m_button = GetComponent<Button>();

//			SetForKey();
        
		    }

		    // Update is called once per frame
		    void Update()
		    {
			//SetForKey();
		    }

		    public void PurchaseMade()
		    {
			//Debug.Log("PurchaseMade");

			//m_lastkeySet = "";
			// SetForKey();

			m_button.interactable = true;
			StartCoroutine (SelectButton (m_button));
		}

		    public void SetForKey()
		    {
			//if (m_lastkeySet != m_selectedKeySR.Value)
			//{
				//m_lastkeySet = m_selectedKeySR.Value;
				//m_button.interactable = m_BTSShop.IsOwned(m_lastkeySet);

			bool l_isOwned = m_BTSShop.IsOwned(m_selectedKeySR.Value);

//			Debug.Log(m_selectedKeySR.Value + " / " + l_isOwned);

			m_button.interactable = l_isOwned;

			//if (l_isOwned) {
			//	m_button.GetComponentInChildren<Text>().text = "Start";
			//}
			//else {
			//	m_button.GetComponentInChildren<Text>().text = "Buy Track";
			//}

			//if ((l_isOwned)) {
			//	StartCoroutine (SelectButton (m_button));
			//}
			//}
		}


		IEnumerator SelectButton (Button a_button)
		{
			yield return new WaitForEndOfFrame ();
			// yield return new WaitForSeconds (1.0f);

			a_button.interactable = true;

			EventSystem.current.SetSelectedGameObject (null);
			a_button.Select ();
			
		}
	}
}
