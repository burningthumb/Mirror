using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestruction : MonoBehaviour
{
    public float m_destroyAfter = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(DestroySelf), m_destroyAfter);

    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
