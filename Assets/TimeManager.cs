using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

public float time = 1f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = time;
    }
}
