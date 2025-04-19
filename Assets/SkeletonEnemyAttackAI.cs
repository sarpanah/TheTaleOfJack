using UnityEngine;

/// <summary>
/// Handles enemy attack logic: hitbox management, attack cooldowns, and dealing damage to the player.
/// </summary>
public class SkeletonAttackAI : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Collider on child used to detect player hits. Must be set as Trigger.")]
    [SerializeField] private Collider2D attackHitbox;
    [Tooltip("Time in seconds between consecutive attacks.")]
    [SerializeField] private float attackCooldown = 1f;
    [Tooltip("Damage dealt to the player on hit.")]
    [SerializeField] private int damageAmount = 15;

    [Header("Animation")]
    [Tooltip("Animator to trigger attack animations.")]
    [SerializeField] private Animator animator;

    private float lastAttackTime;

    private void Start()
    {
        // Validate & initialize
        if (animator == null) animator = GetComponent<Animator>();
        if (attackHitbox == null)
            attackHitbox = GetComponentInChildren<Collider2D>();
        
        if (attackHitbox == null)
            Debug.LogError($"[{name}] AttackHitbox not assigned or found!", this);

        // Ensure hitbox starts disabled
        attackHitbox.enabled = false;
    }

    private void Update()
    {
        // // Example AI check—replace with your own decision logic
        // if (Time.time >= lastAttackTime + attackCooldown && IsPlayerInRange())
        // {
        //     Attack();
        // }
    }

    /// <summary>
    /// Call this (e.g. from AI or animation) to start attack.
    /// </summary>
    public void Attack()
    {
        lastAttackTime = Time.time;
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    // --- Animation Event Hooks ---
    // (Add these to your attack animation at the frames you want the hitbox on/off)
    public void EnableHitbox()
    {
        attackHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        attackHitbox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only apply if we’re in the attack window
       // if (!attackHitbox.enabled) return;

        // Check for player
        if (collision.CompareTag("Player"))
        {
            var playerHealth = collision.GetComponent<PlayerHealthManager>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }

    /// <summary>
    /// Replace or extend this with your actual detection (e.g. Physics2D.OverlapCircle).
    /// </summary>
    private bool IsPlayerInRange()
    {
        // Simple placeholder: always return true
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (attackCooldown < 0f) attackCooldown = 0f;
        if (damageAmount < 0) damageAmount = 0;

        if (attackHitbox == null)
            Debug.LogWarning($"AttackHitbox not assigned on {name}", this);
        if (animator == null)
            Debug.LogWarning($"Animator not assigned on {name}", this);
    }
#endif
}
