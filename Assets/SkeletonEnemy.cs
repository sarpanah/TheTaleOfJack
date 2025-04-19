using UnityEngine;
using System.Collections;

/// <summary>
/// Controls an enemy that patrols between two points in a 2D platformer using Rigidbody2D.
/// Includes flip logic, player detection via raycast, temporary speed boost with adjustable duration,
/// smooth sprite color tint transition, and smooth animation speed adjustment during speed boost.
/// </summary>
public class SkeletonEnemy : MonoBehaviour
{
    #region Inspector Fields
    [Header("Patrol Settings")]
    [SerializeField, Tooltip("Left patrol boundary")]
    private Transform pointA;
    [SerializeField, Tooltip("Right patrol boundary")]
    private Transform pointB;
    [SerializeField, Tooltip("Base movement speed in units per second")]
    private float speed = 2f;

    [Header("Detection Settings")]
    [SerializeField, Tooltip("Distance for player detection raycast")]
    private float detectionRange = 5f;
    [SerializeField, Tooltip("Layers that can block detection or be detected (e.g., Player, Ground)")]
    private LayerMask detectionMask;
    [SerializeField, Tooltip("Duration of speed boost in seconds after detecting player")]
    private float speedBoostDuration = 1.5f;

    [Header("Combat Settings")]
    [SerializeField, Tooltip("Damage dealt to the player on contact")]
    private int damageAmount = 10;
    [SerializeField, Tooltip("Knockback force applied to the player on contact")]
    private float knockbackAmount = 5f;
    [SerializeField, Tooltip("Cooldown between damage instances in seconds")]
    private float damageCooldown = 0.5f;

    [Header("Visual Settings")]
    [SerializeField, Tooltip("Tint color for the sprite during speed boost")]
    private Color boostTintColor = new Color(1f, 0.6f, 0.6f, 1f); // Mild red by default
    [SerializeField, Tooltip("Animation speed multiplier during speed boost")]
    private float animationSpeedMultiplier = 1.3f;
    [SerializeField, Tooltip("Duration of color and animation speed transitions in seconds")]
    private float transitionDuration = 0.3f;

    [Header("Components")]
    [SerializeField, Tooltip("Rigidbody2D for physics-based movement")]
    private Rigidbody2D rb;
    [SerializeField, Tooltip("SpriteRenderer for visual representation")]
    private SpriteRenderer spriteRenderer;
    [SerializeField, Tooltip("Animator for character animations")]
    private Animator animator;
    #endregion

    #region Private Variables
    private Transform currentTarget;        // The point the enemy is moving towards
    private bool facingRight = true;        // Tracks sprite facing direction
    private float speedMultiplier = 1f;     // Multiplier for speed adjustment
    private float damageTimer = 0f;         // Timer for damage cooldown
    private Coroutine speedBoostCoroutine;  // Tracks active speed boost coroutine
    private Color defaultColor;             // Stores the sprite's default color
    private float defaultAnimSpeed;         // Stores the default animation speed
    #endregion

    #region Unity Lifecycle Methods
    /// <summary>
    /// Initializes components and sets the initial target.
    /// </summary>
    private void Start()
    {
        // Ensure Rigidbody2D is assigned
        rb = rb != null ? rb : GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"Rigidbody2D component missing on {gameObject.name}");
            enabled = false;
            return;
        }

        // Ensure SpriteRenderer is assigned
        spriteRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer component missing on {gameObject.name}");
            enabled = false;
            return;
        }

        // Ensure Animator is assigned
        animator = animator != null ? animator : GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Animator component missing on {gameObject.name}");
            enabled = false;
            return;
        }

        // Store default visual settings
        defaultColor = spriteRenderer.color;
        defaultAnimSpeed = animator.speed;

        // Validate patrol points
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"PointA or PointB not assigned on {gameObject.name}");
            enabled = false;
            return;
        }

        // Set initial target
        currentTarget = pointA;

        // Validate detection settings
        if (detectionMask.value == 0)
        {
            Debug.LogWarning($"Detection mask not set on {gameObject.name}. Raycast will detect all layers.");
        }
    }

    /// <summary>
    /// Updates physics-based movement and player detection in fixed time steps.
    /// </summary>
    private void FixedUpdate()
    {
        if (rb == null || currentTarget == null) return;

        // Perform player detection via raycast
        DetectPlayer();

        // Calculate movement direction
        float direction = Mathf.Sign(currentTarget.position.x - transform.position.x);

        // Apply velocity with speed multiplier
        rb.linearVelocity = new Vector2(direction * speed * speedMultiplier, rb.linearVelocity.y);

        // Check if target is reached
        float distanceToTarget = Mathf.Abs(transform.position.x - currentTarget.position.x);
        if (distanceToTarget < 0.1f)
        {
            SwitchTarget();
        }
    }

    /// <summary>
    /// Handles collision with the player for damage and knockback.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || Time.time < damageTimer) return;

        Vector2 knockbackDirection = CalculateKnockbackDirection(collision.transform.position);
        if (collision.gameObject.TryGetComponent<PlayerHealthManager>(out var playerHealth))
        {
            damageTimer = Time.time + damageCooldown;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Casts a ray to detect the player and triggers a temporary speed boost.
    /// </summary>
    private void DetectPlayer()
    {
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -rayDirection, detectionRange, detectionMask);

        // Check if player is detected
        bool playerDetected = hit.collider != null && hit.collider.CompareTag("Player");
        if (playerDetected && speedMultiplier == 1f) // Only trigger if not already boosted
        {
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine); // Stop any existing boost coroutine
            }
            speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine());
        }

        // Visualize raycast for debugging
        Debug.DrawRay(transform.position, -rayDirection * detectionRange, playerDetected ? Color.green : Color.red);
    }

    /// <summary>
    /// Temporarily increases speed, smoothly transitions sprite color and animation speed,
    /// and reverts after the specified duration.
    /// </summary>
    private IEnumerator SpeedBoostRoutine()
    {
        // Apply speed boost immediately
        speedMultiplier = 1.5f;

        // Smoothly transition to boost state
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        float startAnimSpeed = animator.speed;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            // Interpolate color and animation speed
            spriteRenderer.color = Color.Lerp(startColor, boostTintColor, t);
            animator.speed = Mathf.Lerp(startAnimSpeed, defaultAnimSpeed * animationSpeedMultiplier, t);

            yield return null;
        }

        // Ensure final values are exact
        spriteRenderer.color = boostTintColor;
        animator.speed = defaultAnimSpeed * animationSpeedMultiplier;

        // Wait for the remaining boost duration
        yield return new WaitForSeconds(speedBoostDuration - transitionDuration);

        // Smoothly transition back to normal state
        elapsed = 0f;
        startColor = spriteRenderer.color;
        startAnimSpeed = animator.speed;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            // Interpolate color and animation speed
            spriteRenderer.color = Color.Lerp(startColor, defaultColor, t);
            animator.speed = Mathf.Lerp(startAnimSpeed, defaultAnimSpeed, t);

            yield return null;
        }

        // Ensure final values are exact
        spriteRenderer.color = defaultColor;
        animator.speed = defaultAnimSpeed;
        speedMultiplier = 1f;
        speedBoostCoroutine = null;
    }

    /// <summary>
    /// Switches the patrol target and flips the character.
    /// </summary>
    private void SwitchTarget()
    {
        currentTarget = (currentTarget == pointA) ? pointB : pointA;
        Flip();
    }

    /// <summary>
    /// Flips the sprite to face the movement direction.
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    /// <summary>
    /// Calculates knockback direction based on player position.
    /// </summary>
    /// <param name="playerPosition">Player's position.</param>
    /// <returns>Normalized knockback vector.</returns>
    private Vector2 CalculateKnockbackDirection(Vector3 playerPosition)
    {
        Vector2 direction = (playerPosition.x < transform.position.x) ? Vector2.left : Vector2.right;
        return direction.normalized * knockbackAmount;
    }
    #endregion
}