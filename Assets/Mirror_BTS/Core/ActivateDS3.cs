using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateDS3 : MonoBehaviour
{
    [DllImport("ActivateDS3")]
    private static extern void activate();

    private float checkInterval = 1.0f; // Check every 1 second
    private float lastCheckTime = 0f;
    private bool hasActivated = false; // Track if activation has occurred

    void Awake()
    {
        // Ensure only one instance exists
        ActivateDS3[] instances = FindObjectsOfType<ActivateDS3>();
        if (instances.Length > 1)
        {
            // Destroy this instance if another already exists
            Destroy(gameObject);
            return;
        }

        // Persist this instance across scenes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initial check on startup
        CheckForPS3Controller();
    }

    void Update()
    {
        // Check once per second if not yet activated
        if (!hasActivated && Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            CheckForPS3Controller();
        }
    }

    void CheckForPS3Controller()
    {
        var gamepads = Gamepad.all;
        if (gamepads.Count == 0)
        {
            Debug.Log("No gamepads detected.");
            return;
        }

        bool ps3Detected = false;
        foreach (var gamepad in gamepads)
        {
            Debug.Log($"Detected gamepad: {gamepad.name} (ID: {gamepad.deviceId})");
            if (gamepad.name.ToLower().Contains("playstation") || gamepad.name.ToLower().Contains("dualshock"))
            {
                ps3Detected = true;
                break;
            }
        }

        if (ps3Detected)
        {
            TryActivate();
        }
        else
        {
            Debug.Log("No PS3 controller detected among connected gamepads.");
        }
    }

    void TryActivate()
    {
        try
        {
            activate();
            Debug.Log("PS3 controller activation attempted!");
            hasActivated = true; // Stop further checks after successful attempt
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Activation failed: {e.Message}");
        }
    }
}