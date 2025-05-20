using UnityEngine;

/// <summary>
/// Handles player attack logic, including hitbox management, cooldowns, fatigue, and feedback effects.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private PolygonCollider2D attackHitbox;  // Reference to the attack hitbox
    [SerializeField] private float attackCooldown = 0.5f;     // Cooldown between attacks

    [Header("Fatigue Settings")]
    [Tooltip("Time in seconds the player needs to rest without attacking to reset fatigue.")]
    [SerializeField] private float restTime = 1.5f;           // Time to rest to reset fatigue

    [Header("No-Collision Settings")]
    [Tooltip("Tag to assign to the Player GameObject when attack is active")]
    [SerializeField] private string nonCollisionTag = "NoCollision";
    [Tooltip("Layer name to assign to the Player GameObject when attack is active")]
    [SerializeField] private string nonCollisionLayerName = "Ignore Raycast";

    [Header("Input")]
    [SerializeField] private TouchButton attackButton;        // Reference to the attack button
    private bool wasAttackPressedLastFrame = false;  // For edge detection


    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.1f;    // Duration of hit stop in seconds (100ms default)

    private string originalTag;
    private int    originalLayer;
    private float  lastAttackTime;
    private int    fatigueCounter = 0;                        // Counter for consecutive attacks
    private PlayerHealthManager playerHealthManager;
    private Animator            animator;

    public event System.Action<int, int> OnFatigueChanged;  // current, max
    public event System.Action OnFatigueAttackAttempt;      // New event for fatigue attack attempt

    public int MaxFatigue => 4;
    public int CurrentFatigue => fatigueCounter;
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

        // Reset fatigue counter if enough time has passed since last attack
        if (Time.time - lastAttackTime >= restTime && fatigueCounter > 0)
{
    fatigueCounter = 0;
    OnFatigueChanged?.Invoke(fatigueCounter, MaxFatigue);
}


        // Detect initial button press
        bool isAttackJustPressed = attackButton.isPressed && !wasAttackPressedLastFrame;
        wasAttackPressedLastFrame = attackButton.isPressed;

        if (isAttackJustPressed)
        {
            if (Time.time >= lastAttackTime + attackCooldown && fatigueCounter < MaxFatigue)
            {
                // Perform attack and increment fatigue
                Attack();
                fatigueCounter++;
                lastAttackTime = Time.time;
                OnFatigueChanged?.Invoke(fatigueCounter, MaxFatigue);
            }
            else if (fatigueCounter >= MaxFatigue)
            {
                // Player is already fatigued and tried to attack
                OnFatigueAttackAttempt?.Invoke();
            }
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

                float enemyVsPlayerDistance = Vector2.Distance(collision.gameObject.transform.position, transform.position);
                int damage = GetDamageBasedOnDistance(enemyVsPlayerDistance);
                enemyHealth.TakeDamage(damage);
                hitSuccess = true;
                Debug.Log(damage);
                
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

        // Uncomment to trigger camera shake and vibration on a successful hit:
        if (hitSuccess) TriggerFeedbackEffects();
    }

    private int GetDamageBasedOnDistance(float distance)
    {
        if (distance <= 1.2f)
            return UnityEngine.Random.Range(45, 51); // upper bound is exclusive
        else if (distance <= 1.4f)
            return UnityEngine.Random.Range(25, 31);
        else
            return UnityEngine.Random.Range(18, 21);
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
        restTime = Mathf.Max(0f, restTime);  // Ensure restTime is non-negative

        if (attackHitbox == null)
            Debug.LogWarning("Attack Hitbox is not assigned on PlayerAttack.", this);
        if (attackButton == null)
            Debug.LogWarning("Attack Button is not assigned on PlayerAttack.", this);
    }
#endif
}