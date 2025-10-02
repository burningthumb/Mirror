using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSliderValue : MonoBehaviour
{
    public FloatReference m_sliderValue;
    Slider m_slider;
    // Start is called before the first frame update
    void Start()
    {
        m_slider = GetComponent<Slider>();
        m_slider.value = m_sliderValue.Value;

    }

    // Update is called once per frame
    void Update()
    {
        // m_slider.value = m_sliderValue.Value;
    }

    public void ValueChanged()
    {
        m_sliderValue.Value = m_slider.value;
    }

    public void SetSliderFromValue()
    {
	m_slider.value = m_sliderValue.Value;
    }
}
