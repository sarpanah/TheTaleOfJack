using UnityEngine;
using TMPro; // Changed namespace

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Changed type
    public float updateInterval = 0.5f;

    private float accumulatedTime = 0f;
    private int framesCount = 0;

    void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("FPSDisplay: No TextMeshProUGUI component assigned!");
            enabled = false;
        }
    }

    void Update()
    {
        accumulatedTime += Time.unscaledDeltaTime; // Consider using unscaled time
        framesCount++;

        if (accumulatedTime >= updateInterval)
        {
            int fps = Mathf.RoundToInt(framesCount / accumulatedTime);
            fpsText.text = $"FPS: {fps}";

            accumulatedTime = 0f;
            framesCount = 0;
        }
    }
}