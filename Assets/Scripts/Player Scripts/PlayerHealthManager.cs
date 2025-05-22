using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the character.")]
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => maxHealth;

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

    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.1f;    // Duration of hit stop in seconds (100ms default)

    private int currentHealth;
    public int CurrentHealth => currentHealth;
    public bool isDead;
    private Rigidbody2D rb;
    private float knockbackTimer;
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    public void TakeDamage(int damageAmount, Vector2 hitDirection)
    {
        if (isDead) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (HitStopManager.Instance != null)
        {
            HitStopManager.Instance.TriggerHitStop(hitStopDuration);
        }

        animator.SetTrigger("Hit");
        TriggerFeedbackEffects();

        ApplyKnockback(hitDirection);

        if (currentHealth <= 0)
            Die();
    }

    private void ApplyKnockback(Vector2 hitDirection)
    {
        knockbackTimer = knockbackDuration;
        rb.linearVelocity = Vector2.zero;
        Vector2 impulse = hitDirection.normalized * knockbackForce;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    private void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);

        AndroidHapticManager.Instance?.Vibrate(VibrationIntensity.VeryIntense);
        CameraShakeManager.Instance?.ShakeCamera(CameraShakeIntensity.Strong);

        foreach (var script in GetComponents<MonoBehaviour>())
            if (script != this) script.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void TriggerFeedbackEffects()
    {
        if (CameraShakeManager.Instance != null)
            CameraShakeManager.Instance.ShakeCamera(CameraShakeIntensity.VeryLight);
        else
            Debug.LogWarning("CameraShakeManager not found in scene.");

        if (AndroidHapticManager.Instance != null)
            AndroidHapticManager.Instance.Vibrate(VibrationIntensity.Light);
        else
            Debug.LogWarning("AndroidHapticManager not found in scene.");
    }
}