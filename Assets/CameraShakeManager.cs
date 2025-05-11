using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;
    public CinemachineImpulseSource impulseSource;

    [Header("Feedback Effects")]
    [SerializeField] private float shakeDuration = 0.2f;  // Duration of the camera shake
    [SerializeField] private float shakeMagnitude = 0.05f; // Magnitude of the camera shake

    private void Awake()
    {
        Instance = this;
    }

    public void ShakeCamera()
    {
        // Set the impulse parameters dynamically based on the shake's intensity and duration
        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeMagnitude;
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = shakeDuration;

        // Generate an impulse to create the shake
        impulseSource.GenerateImpulse();
    }
}
