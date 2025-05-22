using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    
    public static CameraShakeManager Instance;
    public CinemachineImpulseSource impulseSource;

    private const string PREF_KEY = "CameraShakeEnabled";
    private bool isCameraShakeEnabled = true;

    public bool IsCameraShakeEnabled => isCameraShakeEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        isCameraShakeEnabled = PlayerPrefs.GetInt(PREF_KEY, 1) == 1;
    }

    public void SetCameraShakeEnabled(bool enabled)
    {
        isCameraShakeEnabled = enabled;
        PlayerPrefs.SetInt(PREF_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Shakes the camera using a preset intensity tier.
    /// </summary>
    public void ShakeCamera(CameraShakeIntensity intensity)
    {
        if (!isCameraShakeEnabled || impulseSource == null) return;

        float magnitude;
        float duration;

        switch (intensity)
        {
            case CameraShakeIntensity.VeryLight:
                magnitude = 0.0000005f;
                duration = 0.01f;
                break;
            case CameraShakeIntensity.Light:
                magnitude = 0.05f;
                duration = 0.15f;
                break;
            case CameraShakeIntensity.Medium:
                magnitude = 0.08f;
                duration = 0.2f;
                break;
            case CameraShakeIntensity.Strong:
                magnitude = 0.12f;
                duration = 0.25f;
                break;
            case CameraShakeIntensity.Intense:
                magnitude = 0.18f;
                duration = 0.3f;
                break;
            default:
                magnitude = 0.05f;
                duration = 0.15f;
                Debug.LogWarning("Unknown shake intensity. Defaulting to Light.");
                break;
        }

        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = magnitude;
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;

        impulseSource.GenerateImpulse();
    }
}

/// <summary>
/// Enum defining tiers of camera shake intensity.
/// </summary>
    public enum CameraShakeIntensity
    {
        VeryLight,
        Light,
        Medium,
        Strong,
        Intense
    }
