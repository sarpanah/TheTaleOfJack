using UnityEngine;

/// <summary>
/// Manages the health, damage reactions, knockback, and death behavior for a 2D character.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the character.")]
    [SerializeField] private int maxHealth = 100;

    [Header("Knockback Settings")]
    [Tooltip("Force of knockback applied when hit.")]
    [SerializeField] private float knockbackForce = 10f;
    [Tooltip("Duration during which player control is disabled.")]
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Animation Settings")]
    [Tooltip("Animator component for handling hit and death animations.")]
    [SerializeField] private Animator animator;

    [Header("Death Settings")]
    [Tooltip("Time (in seconds) before the character is destroyed after death.")]
    [SerializeField] private float destroyDelay = 3f;

    private int currentHealth;
    public bool isDead;
    private Rigidbody2D rb;
    private float knockbackTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError($"Rigidbody2D not found on {name}");

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                Debug.LogError($"Animator not assigned on {name}");
        }
    }

    private void Update()
    {
        // Count down knockback duration
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                // Restore control – zero out residual velocity if needed
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    /// <summary>
    /// Applies damage and knockback. Triggers hit animation and checks for death.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply.</param>
    /// <param name="hitDirection">Direction from which the hit came (normalized).</param>
    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        // Trigger hit animation
        animator.SetTrigger("Hit");
        TriggerFeedbackEffects();

        // Apply knockback impulse
        ApplyKnockback(hitDirection);

        if (currentHealth <= 0)
            Die();
    }

    private void ApplyKnockback(Vector2 hitDirection)
    {
        // Disable control for duration
        knockbackTimer = knockbackDuration;

        // Clear existing velocity then apply an impulse opposite to hit direction
        rb.linearVelocity = Vector2.zero;
        Vector2 impulse = hitDirection.normalized * knockbackForce;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    private void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);

        // Haptics & screen shake
        AndroidHapticManager.Instance?.Vibrate(VibrationIntensity.VeryIntense);
        CameraShakeManager.Instance?.ShakeCamera(CameraShakeIntensity.Strong);

        foreach (var script in GetComponents<MonoBehaviour>())
            if (script != this) script.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// Optional: heals the character, clamped at maxHealth.
    /// </summary>
    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    private void TriggerFeedbackEffects()
    {
        // Camera shake
        if (CameraShakeManager.Instance != null)
            CameraShakeManager.Instance.ShakeCamera(CameraShakeIntensity.Medium);
        else
            Debug.LogWarning("CameraShakeManager not found in scene.");

        // Vibration
        if (AndroidHapticManager.Instance != null)
            AndroidHapticManager.Instance.Vibrate(VibrationIntensity.Light);
        else
            Debug.LogWarning("AndroidHapticManager not found in scene.");
    }
}
