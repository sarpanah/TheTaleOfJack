using UnityEngine;

public class FrameRateSetter : MonoBehaviour
{
    void Awake()
    {
        // Set the target frame rate to 60 FPS
        Application.targetFrameRate = 60;
    }
}
