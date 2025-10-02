
using UnityEngine;
using UnityEngine.UI;

public class SetTextFromString : MonoBehaviour
{
    // Start is called before the first frame update
    public StringReference m_StringReference;

    void Update()
    {
        gameObject.GetComponent<Text>().text = m_StringReference.Value;
    }
}
