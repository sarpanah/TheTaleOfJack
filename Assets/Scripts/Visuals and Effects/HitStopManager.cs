using UnityEngine;
using System.Collections;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    [SerializeField] private float hitStopTimeScale = 0f; // 0 for full stop, can be adjusted for slow motion

    private float hitStopEndTime = 0f;
    private bool isHitStopping = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerHitStop(float duration)
    {
        if (duration > 0)
        {
            float endTime = Time.unscaledTime + duration;
            if (endTime > hitStopEndTime)
            {
                hitStopEndTime = endTime;
                if (!isHitStopping)
                {
                    StartCoroutine(HitStopCoroutine());
                }
            }
        }
    }

    private IEnumerator HitStopCoroutine()
    {
        isHitStopping = true;
        Time.timeScale = hitStopTimeScale;
        while (Time.unscaledTime < hitStopEndTime)
        {
            yield return null;
        }
        Time.timeScale = 1f;
        isHitStopping = false;
    }
}