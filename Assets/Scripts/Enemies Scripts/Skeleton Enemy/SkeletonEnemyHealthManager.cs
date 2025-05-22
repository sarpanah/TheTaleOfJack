using System;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// Manages the health, damage reactions, and death behavior for a Skeleton enemy.
/// </summary>
public class SkeletonEnemyHealthManager : MonoBehaviour
{
    [Header("Health Settings")]    
    [Tooltip("Maximum health of the skeleton.")]
    [SerializeField] private int maxHealth = 100; public int MaxHealth => maxHealth;
    private int currentHealth;  public int CurrentHealth => currentHealth;

    [Header("Animation Settings")]    
    [Tooltip("Animator component for handling hit and death animations.")]
    [SerializeField] private Animator animator;

    [Header("Death Settings")]    
    [Tooltip("Time (in seconds) before the skeleton is destroyed after death.")]
    [SerializeField] private float destroyDelay = 3f;

    [Header("No-Collision Settings")]
    [Tooltip("Tag to assign to the Player GameObject when attack is active")]
    [SerializeField] private string nonCollisionTag = "NoCollision";

    public GameObject coinPrefab;

    private bool isDead;


    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnDamaged;
    private void Awake()
    {
        // Initialize health and state
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke(damageAmount);
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
        animator.SetBool("isDead", isDead);

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
                gameObject.tag = nonCollisionTag;
        }

        SetTagAndLayerRecursively(this.gameObject, nonCollisionTag, nonCollisionTag);
        Instantiate(coinPrefab, transform.position, Quaternion.identity);
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


    void SetTagAndLayerRecursively(GameObject obj, string tag, string layerName)
    {
        obj.tag = tag;

        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerName}' does not exist.");
            return;
        }

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetTagAndLayerRecursively(child.gameObject, tag, layerName);
        }
    }

    private void TriggerFeedbackEffects()
        {
            Debug.Log("SHIT CALLED");
            // Camera shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeCamera(CameraShakeIntensity.VeryLight);
            }
            else
            {
                Debug.LogWarning("CameraShakeManager not found in scene.");
            }

            // Vibration (light tier)
            if (AndroidHapticManager.Instance != null)
            {
                AndroidHapticManager.Instance.Vibrate(VibrationIntensity.Light);
            }
            else
            {
                Debug.LogWarning("AndroidHapticManager not found in scene.");
            }
    }
}
