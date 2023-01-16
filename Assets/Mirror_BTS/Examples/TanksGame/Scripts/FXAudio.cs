using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXAudio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Play(gameObject.name, AudioManager.MixerTarget.SFX);
    }
}
