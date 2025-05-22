using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HapticsToggleBinder : MonoBehaviour
{
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        // Initialize toggle state from the manager
        if (AndroidHapticManager.Instance != null)
            _toggle.isOn = AndroidHapticManager.Instance.IsHapticsEnabled;
        else
            _toggle.isOn = false;

        // Listen for UI changes
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        Debug.Log("ToggleChanged!");
        if (AndroidHapticManager.Instance != null)
            AndroidHapticManager.Instance.SetHapticsEnabled(isOn);
        else
            Debug.LogWarning("AndroidHapticManager instance not found.");
    }

    void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
