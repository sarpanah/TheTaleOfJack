using UnityEngine;
using System.Collections;

public class AndroidHapticManager : MonoBehaviour
{
    public static AndroidHapticManager Instance { get; private set; }

    private const string PREFS_KEY = "HapticsEnabled";

    private const long VERY_LIGHT_VIBRATION_MS   = 15;
    private const long LIGHT_VIBRATION_MS        = 80;
    private const long MEDIUM_VIBRATION_MS       = 150;
    private const long SEVERE_VIBRATION_MS       = 300;
    private const long VERY_INTENSE_VIBRATION_MS = 600;

    private bool platformSupportsVibration;
    public bool IsHapticsEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeVibration();
        LoadHapticsSetting();
    }

    private void InitializeVibration()
    {
#if UNITY_ANDROID
        platformSupportsVibration = true;
#else
        platformSupportsVibration = false;
#endif
        if (!platformSupportsVibration)
            Debug.Log("Vibration not supported on this platform.");
    }

    private void LoadHapticsSetting()
    {
        IsHapticsEnabled = PlayerPrefs.GetInt(PREFS_KEY, 1) == 1;
    }

    private void SaveHapticsSetting()
    {
        PlayerPrefs.SetInt(PREFS_KEY, IsHapticsEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Allows the player to enable or disable haptic feedback.
    /// </summary>
    public void SetHapticsEnabled(bool enabled)
    {
        IsHapticsEnabled = enabled;
        SaveHapticsSetting();
        Debug.Log($"Haptics turned {(enabled ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Triggers device vibration based on intensity if allowed.
    /// </summary>
    public void Vibrate(VibrationIntensity intensity)
    {
        if (!platformSupportsVibration || !IsHapticsEnabled) return;

        long duration;
        int repeatCount;

        switch (intensity)
        {
            case VibrationIntensity.VeryLight:
                duration = VERY_LIGHT_VIBRATION_MS; repeatCount = 1; break;
            case VibrationIntensity.Light:
                duration = LIGHT_VIBRATION_MS; repeatCount = 1; break;
            case VibrationIntensity.Medium:
                duration = MEDIUM_VIBRATION_MS; repeatCount = 2; break;
            case VibrationIntensity.Severe:
                duration = SEVERE_VIBRATION_MS; repeatCount = 3; break;
            case VibrationIntensity.VeryIntense:
                duration = VERY_INTENSE_VIBRATION_MS; repeatCount = 4; break;
            default:
                Debug.LogWarning("Unknown vibration intensity."); return;
        }

        if (!TryCustomVibration(duration))
            StartCoroutine(VibrateMultipleTimes(repeatCount));
    }

    private bool TryCustomVibration(long milliseconds)
    {
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
            {
                Debug.LogWarning("Vibrator unavailable or device lacks vibrator.");
                return false;
            }

            vibrator.Call("vibrate", milliseconds);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Custom vibration failed: {e.Message}. Falling back.");
            return false;
        }
    }

    private IEnumerator VibrateMultipleTimes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Handheld.Vibrate();
            if (i < count - 1)
                yield return new WaitForSeconds(0.1f);
        }
    }
}

public enum VibrationIntensity
{
    VeryLight,
    Light,
    Medium,
    Severe,
    VeryIntense
}
