using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CameraShakeToggleBinder : MonoBehaviour
{
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        if (CameraShakeManager.Instance != null)
            _toggle.isOn = CameraShakeManager.Instance.IsCameraShakeEnabled;

        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (CameraShakeManager.Instance != null)
            CameraShakeManager.Instance.SetCameraShakeEnabled(isOn);
    }

    void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
