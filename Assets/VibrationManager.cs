using UnityEngine;
using System.Collections;

public class VibrationManager : MonoBehaviour
{
    // Singleton instance
    public static VibrationManager Instance { get; private set; }

    // Vibration durations in milliseconds
    private const long VERY_LIGHT_VIBRATION_MS = 15;
    private const long LIGHT_VIBRATION_MS      = 80;
    private const long MEDIUM_VIBRATION_MS     = 150;
    private const long SEVERE_VIBRATION_MS     = 300;
    private const long VERY_INTENSE_VIBRATION_MS = 600;

    private bool canVibrate;

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
    }

    private void InitializeVibration()
    {
#if UNITY_ANDROID || UNITY_IOS
        canVibrate = true;
#else
        canVibrate = false;
#endif
        if (!canVibrate)
            Debug.Log("Vibration not supported on this platform.");
    }

    /// <summary>
    /// Triggers a vibration based on the specified intensity tier.
    /// </summary>
    public void Vibrate(VibrationIntensity intensity)
    {
        if (!canVibrate) return;

        long duration;
        int repeatCount;

        switch (intensity)
        {
            case VibrationIntensity.VeryLight:
                duration    = VERY_LIGHT_VIBRATION_MS;
                repeatCount = 1;
                break;
            case VibrationIntensity.Light:
                duration    = LIGHT_VIBRATION_MS;
                repeatCount = 1;
                break;
            case VibrationIntensity.Medium:
                duration    = MEDIUM_VIBRATION_MS;
                repeatCount = 2;
                break;
            case VibrationIntensity.Severe:
                duration    = SEVERE_VIBRATION_MS;
                repeatCount = 3;
                break;
            case VibrationIntensity.VeryIntense:
                duration    = VERY_INTENSE_VIBRATION_MS;
                repeatCount = 4;
                break;
            default:
                Debug.LogWarning("Unknown vibration intensity.");
                return;
        }

#if UNITY_ANDROID
        if (!TryCustomVibration(duration))
            StartCoroutine(VibrateMultipleTimes(repeatCount));
#elif UNITY_IOS
        StartCoroutine(VibrateMultipleTimes(repeatCount));
#endif
    }

#if UNITY_ANDROID
    private bool TryCustomVibration(long milliseconds)
    {
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer?.GetStatic<AndroidJavaObject>("currentActivity");
            var vibrator = activity?.Call<AndroidJavaObject>("getSystemService", "vibrator");

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
#endif

    private IEnumerator VibrateMultipleTimes(int count)
    {
        for (int i = 0; i < count; i++)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            if (i < count - 1)
                yield return new WaitForSeconds(0.1f);
        }
    }
}

/// <summary>
/// Enum defining the tiers of vibration intensity.
/// </summary>
public enum VibrationIntensity
{
    VeryLight,
    Light,
    Medium,
    Severe,
    VeryIntense
}
