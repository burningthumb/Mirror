using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BTSInteractableAfterTime : MonoBehaviour
{
    [SerializeField] float m_time = 30;
    [SerializeField] Button m_button;
    [SerializeField] TMP_Text m_hintText;

    string m_saveText;

    public void Start()
    {
        if (null == m_button)
        {
            m_button = GetComponent<Button>();
        }

        if (null != m_button)
        {
            m_button.interactable = false;
            //m_button.enabled = false;
        }

        if (null != m_hintText)
        {
            m_saveText = m_hintText.text;
        }

        // Start the countdown using a coroutine instead of Invoke
        StartCoroutine(CountdownCoroutine());
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        float remainingTime = m_time;

        while (remainingTime > 0)
        {
            // Wait for 1 second using unscaled time
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime >= 1.0f);

            remainingTime--;

            if (null != m_hintText)
            {
                m_hintText.text = $"{remainingTime} Seconds";
            }
        }

        // When countdown reaches 0, enable the button and restore text
        if (null != m_button)
        {
            m_button.interactable = true;
            //m_button.enabled = true;
        }

        if (null != m_hintText)
        {
            m_hintText.text = m_saveText;
        }
    }
}
