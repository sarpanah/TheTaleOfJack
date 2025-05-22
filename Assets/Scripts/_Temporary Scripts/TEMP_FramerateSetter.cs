using UnityEngine;

public class FrameRateSetter : MonoBehaviour
{
    public int frameRate = 60;
    void Awake()
    {
        // Set the target frame rate to 60 FPS
        Application.targetFrameRate = frameRate;
    }
}
