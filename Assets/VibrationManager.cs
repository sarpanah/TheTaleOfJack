using UnityEngine;
using System.Collections;

public class VibrationManager : MonoBehaviour
{
    // Singleton instance
    public static VibrationManager Instance { get; private set; }

    // Vibration durations in milliseconds (used as a reference for Android custom vibration)
    private const long LIGHT_VIBRATION_MS = 50;
    private const long MEDIUM_VIBRATION_MS = 150;
    private const long SEVERE_VIBRATION_MS = 300;

    private bool canVibrate;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeVibration();
    }

    /// <summary>
    /// Initializes vibration support based on platform availability.
    /// </summary>
    private void InitializeVibration()
    {
#if UNITY_ANDROID || UNITY_IOS
        canVibrate = true; // Assume vibration is supported on mobile platforms
#else
        canVibrate = false; // Disable in editor or unsupported platforms
#endif
        if (!canVibrate)
        {
            Debug.Log("Vibration not supported on this platform.");
        }
    }

    /// <summary>
    /// Triggers a vibration based on the specified intensity tier.
    /// </summary>
    /// <param name="intensity">The vibration intensity level.</param>
    public void Vibrate(VibrationIntensity intensity)
    {
        if (!canVibrate) return;

        int repeatCount = 0;
        long duration = 0;

        // Define vibration parameters based on intensity
        switch (intensity)
        {
            case VibrationIntensity.Light:
                duration = LIGHT_VIBRATION_MS;
                repeatCount = 1;
                break;
            case VibrationIntensity.Medium:
                duration = MEDIUM_VIBRATION_MS;
                repeatCount = 2;
                break;
            case VibrationIntensity.Severe:
                duration = SEVERE_VIBRATION_MS;
                repeatCount = 3;
                break;
            default:
                Debug.LogWarning("Unknown vibration intensity.");
                return;
        }

#if UNITY_ANDROID
        // Try custom vibration first, fall back to repeated Handheld.Vibrate() if it fails
        if (!TryCustomVibration(duration))
        {
            StartCoroutine(VibrateMultipleTimes(repeatCount));
        }
#elif UNITY_IOS
        // iOS only supports basic vibration, so simulate intensity with repeats
        StartCoroutine(VibrateMultipleTimes(repeatCount));
#else
        // Do nothing on unsupported platforms
#endif
    }

#if UNITY_ANDROID
    /// <summary>
    /// Attempts to trigger a custom vibration duration on Android.
    /// Returns true if successful, false if it fails.
    /// </summary>
    private bool TryCustomVibration(long milliseconds)
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                if (unityPlayer == null)
                {
                    Debug.LogWarning("UnityPlayer class not found.");
                    return false;
                }

                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (activity == null)
                    {
                        Debug.LogWarning("Current activity is null.");
                        return false;
                    }

                    using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    {
                        if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
                        {
                            Debug.LogWarning("Vibrator service unavailable or device lacks vibrator.");
                            return false;
                        }

                        vibrator.Call("vibrate", milliseconds);
                        return true;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Custom Android vibration failed: {e.Message}. Falling back to default vibration.");
            return false;
        }
    }
#endif

    /// <summary>
    /// Coroutine to simulate vibration intensity by repeating Handheld.Vibrate() with delays.
    /// </summary>
    private IEnumerator VibrateMultipleTimes(int count)
    {
        for (int i = 0; i < count; i++)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            if (i < count - 1)
            {
                yield return new WaitForSeconds(0.1f); // 100ms delay between vibrations
            }
        }
    }
}

/// <summary>
/// Enum defining the tiers of vibration intensity.
/// </summary>
public enum VibrationIntensity
{
    Light,
    Medium,
    Severe
}