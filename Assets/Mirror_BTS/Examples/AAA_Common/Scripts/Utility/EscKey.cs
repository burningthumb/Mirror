using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EscKey : MonoBehaviour
{
    [SerializeField] Canvas[] m_canvases;
    [SerializeField] Button m_show;
    [SerializeField] Button m_hide;

    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isMobilePlatform)
        {
            m_canvases = FindObjectsOfType<Canvas>();

            foreach (Canvas l_canvas in m_canvases)
            {
                l_canvas.gameObject.SetActive(false);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isMobilePlatform)
        {
            bool l_didToggleVisible = false;

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                foreach (Canvas l_canvas in m_canvases)
                {
                    l_didToggleVisible = !l_canvas.gameObject.activeSelf;

                    l_canvas.gameObject.SetActive(l_didToggleVisible);
                }
            }

            if (l_didToggleVisible)
            {
                // Wait for the end of the frame so that the button game object state
                // is ready to be queried
                StartCoroutine(SelectActiveButton());
            }
        }

    }

    IEnumerator SelectActiveButton()
    {
        yield return new WaitForEndOfFrame();

        if (m_hide.gameObject.activeSelf)
        {
            m_hide.Select();
        }
        else
        {
            m_show.Select();
        }
    }
}
