using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleLevelManager : MonoBehaviour
{
    public void LoadGameScene(string a_scene)
    {
        SceneManager.LoadScene(a_scene);
    }
}
