using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SOString", menuName 
= "SO/String")]
public class SOString : ScriptableObject
{
    // 17fb53745
    [SerializeField] string m_value;

    public string Value
    {
        get
        {
            return m_value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
