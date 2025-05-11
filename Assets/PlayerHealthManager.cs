using UnityEngine;

/// <summary>
/// Manages the health, damage reactions, and death behavior for a Skeleton enemy.
/// </summary>
public class PlayerHealthManager : MonoBehaviour
{
    [Header("Health Settings")]    
    [Tooltip("Maximum health of the skeleton.")]
    [SerializeField] private int maxHealth = 100;

    [Header("Animation Settings")]    
    [Tooltip("Animator component for handling hit and death animations.")]
    [SerializeField] private Animator animator;

    [Header("Death Settings")]    
    [Tooltip("Time (in seconds) before the skeleton is destroyed after death.")]
    [SerializeField] private float destroyDelay = 3f;

    private int currentHealth;
    public bool isDead;

    private void Awake()
    {
        // Initialize health and state
        currentHealth = maxHealth;
        isDead = false;

        // Ensure animator is assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator not assigned on " + name);
            }
        }
    }

    /// <summary>
    /// Applies damage to the skeleton. Triggers hit animation and checks for death.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply.</param>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        // Play hit reaction animation
        animator.SetTrigger("Hit");

        TriggerFeedbackEffects();

        // Optionally, you could play a sound effect or spawn particles here
        // e.g., AudioSource.PlayClipAtPoint(hitSound, transform.position);

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles death logic: triggers death animation, disables further interactions, and schedules destruction.
    /// </summary>
    private void Die()
    {
        isDead = true;

        // Trigger death animation
        animator.SetBool("IsDead", true);

        // Disable collider to prevent further hits
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Disable any movement or AI scripts here if present
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        // Destroy the game object after a delay to allow death animation to play
        Destroy(gameObject, destroyDelay);
    }

    #region Optional Utility
    // Example of a public method to heal the skeleton (could be called from other scripts)
    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }
    #endregion

    private void TriggerFeedbackEffects()
        {
            Debug.Log("SHIT CALLED");
            // Camera shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeCamera();
            }
            else
            {
                Debug.LogWarning("CameraShakeManager not found in scene.");
            }

            // Vibration (light tier)
            if (VibrationManager.Instance != null)
            {
                VibrationManager.Instance.Vibrate(VibrationIntensity.Light);
            }
            else
            {
                Debug.LogWarning("VibrationManager not found in scene.");
            }
    }
}
