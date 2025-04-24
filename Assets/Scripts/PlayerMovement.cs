using UnityEngine;

/// <summary>
/// Handles player movement, jumping, and interactions for a 2D platformer game.
/// Supports an arbitrary number of wall check points and subtle wall boost.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Movement Settings")]
    [SerializeField, Tooltip("Horizontal movement speed")] private float moveSpeed = 5f;
    [SerializeField, Tooltip("Deceleration when no input is given (meters/second²)")] private float deceleration = 10f;

    [Header("Jump Settings")]
    [SerializeField, Tooltip("Initial jump velocity (units/sec)")] private float jumpVelocity = 12f;
    [SerializeField, Tooltip("Initial coyote time window")] private float coyoteTime = 0.2f;
    [SerializeField, Tooltip("Time window to buffer jump input")] private float jumpBufferTime = 0.2f;

    [Header("Gravity Settings")]
    [SerializeField, Tooltip("Base gravity scale on Rigidbody2D")] private float baseGravityScale = 3f;
    [SerializeField, Tooltip("Gravity multiplier when falling")] private float fallMultiplier = 2.5f;
    [SerializeField, Tooltip("Gravity multiplier for low jumps (when jump released early)")] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField, Tooltip("Transform for ground checking")] private Transform groundCheck;
    [SerializeField, Tooltip("Radius for ground check detection")] private float groundCheckRadius = 0.2f;
    [SerializeField, Tooltip("Layer mask for ground objects")] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField, Tooltip("Transforms for wall checking (can assign 1..N points)")] private Transform[] wallCheckPoints;
    [SerializeField, Tooltip("Distance for wall detection")] private float wallCheckDistance = 0.2f;
    [SerializeField, Tooltip("Layer mask for wall objects")] private LayerMask wallLayer;

    [Header("Wall Boost Settings")]
    [SerializeField, Tooltip("Vertical boost applied when hitting the wall at the middle point")] private float wallBoostVelocity = 1f;

    [Header("Touch Controls")]
    [SerializeField, Tooltip("Button for moving left")] private TouchButton moveLeftButton;
    [SerializeField, Tooltip("Button for moving right")] private TouchButton moveRightButton;
    [SerializeField, Tooltip("Button for jumping")] private TouchButton jumpButton;

    #endregion

    #region Private Variables

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool canDoubleJump;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpButtonHeld;
    private bool jumpButtonReleased = true;
    private bool isControlEnabled = true;
    private float fallTime = 0f;
    private bool isTouchingWall;

    // Tracks if boost has been applied during current wall touch
    private bool wallBoostApplied;

    private const float fallThreshold = -0.1f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = baseGravityScale;  // Set base gravity
    }

    private void Update()
    {
        if (!isControlEnabled)
        {
            movement = Vector2.zero;
            animator.SetBool("isRunning", false);
            return;
        }

        UpdateGroundedState();
        UpdateWallCollision();
        HandleJumpTiming();
        HandleJumping();
        HandleInput();
        HandleAnimationAndFlipping();
    }

    private void FixedUpdate()
    {
        if (!isControlEnabled) return;

        if (rb.linearVelocity.y < 0f)
            fallTime += Time.fixedDeltaTime;
        else
            fallTime = 0f;

        ApplyMovement();
        HandleVariableGravity();  // Polished gravity adjustments
    }

    #endregion

    #region Movement Logic

    private void HandleInput()
    {
        if (moveLeftButton == null || moveRightButton == null) return;

        movement.x = moveLeftButton.isPressed ? -1f :
                     moveRightButton.isPressed ? 1f : 0f;

        if (isTouchingWall && MovingTowardsWall())
            movement.x = 0f;
    }

    private void ApplyMovement()
    {
        if (isTouchingWall && ((rb.linearVelocity.x > 0f && isFacingRight) || (rb.linearVelocity.x < 0f && !isFacingRight)))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else if (movement.x != 0f)
        {
            rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            float vx = Mathf.MoveTowards(rb.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        }
    }

    private void HandleAnimationAndFlipping()
    {
        bool isRunning = Mathf.Abs(movement.x) > 0.01f;
        animator.SetBool("isRunning", isRunning);

        float vy = rb.linearVelocity.y;
        bool isJumping = vy > 0.1f;
        bool isFalling  = vy < fallThreshold;

        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);

        if (movement.x > 0f && !isFacingRight) Flip();
        else if (movement.x < 0f && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    #endregion

    #region Jump Logic

    private void HandleJumpTiming()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
            jumpBufferCounter = 0f;
        }
        else coyoteTimeCounter -= Time.deltaTime;

        if (jumpButton != null)
        {
            if (jumpButton.isPressed && jumpButtonReleased && !jumpButtonHeld)
            {
                jumpBufferCounter = jumpBufferTime;
                jumpButtonHeld = true;
            }
            else if (!jumpButton.isPressed)
            {
                jumpButtonHeld = false;
                jumpButtonReleased = true;
            }
        }
    }

    private void HandleJumping()
    {
        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f)
            {
                Jump(false); // Initial jump
                jumpBufferCounter = 0f;
            }
            else if (canDoubleJump)
            {
                Jump(true); // Double jump
                canDoubleJump = false;
                jumpBufferCounter = 0f;
            }
        }

        if (isGrounded) jumpBufferCounter = 0f;
    }

    private void Jump(bool isDoubleJump)
    {
        float v = isDoubleJump ? jumpVelocity * 0.8f : jumpVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, v);
        jumpButtonReleased = false;
    }

    #endregion

    #region Variable Gravity

    /// <summary>
    /// Applies fall and low-jump multipliers for snappier jumps.
    /// </summary>
    private void HandleVariableGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            // Faster fall
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !(jumpButton != null && jumpButton.isPressed))
        {
            // Short hop when jump released early
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Collision Detection

    private void UpdateGroundedState()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundLayer);
    }

    private void UpdateWallCollision()
    {
        bool touchingWall = false;
        int hitIndex = -1;
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        for (int i = 0; i < wallCheckPoints.Length; i++)
        {
            Transform point = wallCheckPoints[i];
            if (point == null) continue;
            if (Physics2D.Raycast(point.position, dir, wallCheckDistance, wallLayer))
            {
                touchingWall = true;
                hitIndex = i;
                break;
            }
        }

        // Reset boost when leaving wall
        if (!touchingWall)
            wallBoostApplied = false;

        isTouchingWall = touchingWall;

        // Subtle boost when hitting middle point
        int middleIndex = wallCheckPoints.Length / 2;
        if (touchingWall && !wallBoostApplied && hitIndex == middleIndex)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + wallBoostVelocity);
            wallBoostApplied = true;
        }
    }

    private bool MovingTowardsWall()
    {
        return (movement.x > 0f && isFacingRight) || (movement.x < 0f && !isFacingRight);
    }

    #endregion

    #region Control Management

    public void DisableControl()
    {
        isControlEnabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void EnableControl()
    {
        isControlEnabled = true;
    }

    public void FreezePlayer()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (wallCheckPoints != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 dir = Application.isPlaying && isFacingRight ? Vector2.right : Vector2.left;
            for (int i = 0; i < wallCheckPoints.Length; i++)
            {
                Transform point = wallCheckPoints[i];
                if (point == null) continue;
                Gizmos.DrawLine(point.position, point.position + (Vector3)dir * wallCheckDistance);
                // Visualize middle point
                if (i == wallCheckPoints.Length / 2)
                    Gizmos.DrawWireSphere(point.position, 0.05f);
            }
        }
    }

    #endregion
}
