using UnityEngine;
using UnityEngine.InputSystem;

public class RawInputTest : MonoBehaviour {
    void Start() {
        foreach (var device in InputSystem.devices) {
            Debug.Log($"Device: {device.name} | {device.description}");
        }
    }

    void OnEnable() {
        InputSystem.onDeviceChange += (device, change) => {
            Debug.Log($"Device Change: {device.name} - {change}");
        };
    }
}