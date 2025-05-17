using UnityEngine;

/// <summary>
/// Handles player attack logic, including hitbox management, cooldowns, and feedback effects.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private PolygonCollider2D attackHitbox;  // Reference to the attack hitbox
    [SerializeField] private float attackCooldown = 0.5f;     // Cooldown between attacks

    [Header("No-Collision Settings")]
    [Tooltip("Tag to assign to the Player GameObject when attack is active")]
    [SerializeField] private string nonCollisionTag = "NoCollision";
    [Tooltip("Layer name to assign to the Player GameObject when attack is active")]
    [SerializeField] private string nonCollisionLayerName = "Ignore Raycast";

    [Header("Input")]
    [SerializeField] private TouchButton attackButton;        // Reference to the attack button

    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.1f;    // Duration of hit stop in seconds (100ms default)

    private string originalTag;
    private int    originalLayer;
    private float  lastAttackTime;
    private PlayerHealthManager playerHealthManager;
    private Animator            animator;

    private void Start()
    {
        InitializeComponents();
        CacheOriginalSettings();
    }
    
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator component missing on PlayerAttack.", this);

        playerHealthManager = GetComponent<PlayerHealthManager>();
        if (playerHealthManager == null)
            Debug.LogError("PlayerHealthManager component missing on PlayerAttack.", this);

        if (attackHitbox == null)
        {
            attackHitbox = GetComponent<PolygonCollider2D>();
            if (attackHitbox == null)
                Debug.LogError("Attack Hitbox (PolygonCollider2D) not assigned or found.", this);
        }

        // Ensure hitbox starts disabled
        attackHitbox.enabled = false;
    }

    private void CacheOriginalSettings()
    {
        originalTag   = gameObject.tag;
        originalLayer = gameObject.layer;
    }

    private void Update()
    {
        if (playerHealthManager == null || playerHealthManager.isDead) return;
        if (attackButton == null || animator == null)             return;

        if (attackButton.isPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            attackButton.isPressed = false;
        }
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
    }

    public void EnableHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = true;

        gameObject.tag = nonCollisionTag;
        int layerIdx = LayerMask.NameToLayer(nonCollisionLayerName);
        if (layerIdx >= 0)
            gameObject.layer = layerIdx;
        else
            Debug.LogWarning($"Layer '{nonCollisionLayerName}' not found in Project Settings.", this);
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;

        gameObject.tag   = originalTag;
        gameObject.layer = originalLayer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!attackHitbox.enabled) return;

        bool hitSuccess = false;

        if (collision.CompareTag("Enemy"))
        {
            var enemyHealth = collision.GetComponent<SkeletonEnemyHealthManager>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(20);
                hitSuccess = true;
            }
        }

        if (collision.CompareTag("Box"))
        {
            var box = collision.GetComponent<Box>();
            if (box != null)
            {
                Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                box.BreakBox(attackDirection);
            }
        }

        if (hitSuccess && HitStopManager.Instance != null)
        {
            HitStopManager.Instance.TriggerHitStop(hitStopDuration);
        }

        //Uncomment to trigger camera shake and vibration on a successful hit:
        if (hitSuccess) TriggerFeedbackEffects();
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        attackCooldown = Mathf.Max(0f, attackCooldown);
        hitStopDuration = Mathf.Max(0f, hitStopDuration);

        if (attackHitbox == null)
            Debug.LogWarning("Attack Hitbox is not assigned on PlayerAttack.", this);
        if (attackButton == null)
            Debug.LogWarning("Attack Button is not assigned on PlayerAttack.", this);
    }
#endif
}