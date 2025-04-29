using UnityEngine;

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
    [SerializeField, Tooltip("Button for moving up")] private TouchButton moveUpButton;
    [SerializeField, Tooltip("Button for moving down")] private TouchButton moveDownButton;

    [Header("Climbing Settings")]
    [SerializeField, Tooltip("Vertical climbing speed")] private float climbSpeed = 3f;

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
    private bool wallBoostApplied;
    private bool isClimbing = false;
    private bool canClimb = false;
    private Vector2 wallDirection;

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
        // Check for climbing activation
        if (!isClimbing && canClimb && movement.x != 0f)
        {
            // Climb only if player is pressing toward the wall's direction
            if ((movement.x > 0f && wallDirection.x > 0f) || (movement.x < 0f && wallDirection.x < 0f))
            {
                isClimbing = true;
                rb.gravityScale = 6.5f;
                animator.SetBool("isClimbing", true);
            }
        }

        UpdateGroundedState();

        // Exit climbing if grounded
        if (isClimbing && isGrounded)
        {
            isClimbing = false;
            rb.gravityScale = baseGravityScale;
            animator.SetBool("isClimbing", false);
        }

        UpdateWallCollision();
        HandleJumpTiming();
        HandleJumping();
        HandleInput();

        // Enter climbing if overlapping climbable wall and pressing towards it
        if (!isClimbing && canClimb && movement.x != 0f && (movement.x > 0f == (wallDirection.x > 0f)))
        {
            isClimbing = true;
            rb.gravityScale = 6.5f;
            animator.SetBool("isClimbing", true);
        }

        HandleAnimationAndFlipping();
    }

    private void FixedUpdate()
    {
        if (!isControlEnabled) return;

        if (rb.linearVelocity.y < 0f && !isClimbing)
            fallTime += Time.fixedDeltaTime;
        else
            fallTime = 0f;

        ApplyMovement();
        if (!isClimbing) HandleVariableGravity();  // Skip gravity adjustments when climbing
    }
    #endregion

    #region Colliders & Triggers

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("ClimbableWall"))
    {
        canClimb = true;
        BoxCollider2D wallCollider = other.GetComponent<BoxCollider2D>();
        if (wallCollider != null)
        {
            Vector2 playerPos = transform.position;
            Vector2 wallCenter = wallCollider.bounds.center;
            float wallLeft = wallCenter.x - wallCollider.bounds.extents.x;
            float wallRight = wallCenter.x + wallCollider.bounds.extents.x;

            // Determine which side of the wall the player is on
            if (playerPos.x < wallLeft)
            {
                wallDirection = Vector2.right; // Player is left of wall, wall is to the right
            }
            else if (playerPos.x > wallRight)
            {
                wallDirection = Vector2.left; // Player is right of wall, wall is to the left
            }
            else
            {
                // Player is within the wall's bounds (e.g., overlapping), use movement direction
                wallDirection = movement.x > 0 ? Vector2.right : Vector2.left;
            }
        }
    }
}

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Removed automatic climbing activation here
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ClimbableWall"))
        {
            canClimb = false;
            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = baseGravityScale;
                animator.SetBool("isClimbing", false);
            }
        }
    }

    #endregion

    #region Movement Logic

    private void HandleInput()
    {
        if (moveLeftButton == null || moveRightButton == null || moveUpButton == null || moveDownButton == null) return;

        movement.x = moveLeftButton.isPressed ? -1f :
                     moveRightButton.isPressed ? 1f : 0f;

        movement.y = moveUpButton.isPressed ? 1f :
                     moveDownButton.isPressed ? -1f : 0f;

        if (isTouchingWall && MovingTowardsWall() && !isClimbing)
            movement.x = 0f;
    }

    private void ApplyMovement()
    {
        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0f, movement.y * climbSpeed);
        }
        else if (isTouchingWall && ((rb.linearVelocity.x > 0f && isFacingRight) || (rb.linearVelocity.x < 0f && !isFacingRight)))
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
        if (isClimbing)
        {
            bool IsClimbing = rb.linearVelocity.y != 0f;
            animator.SetBool("isClimbing", true);
            animator.SetBool("isJumping", false);
        }
        else
        {
            animator.SetBool("isClimbing", false);
            bool isRunning = movement.x != 0f;
            animator.SetBool("isRunning", isRunning);
            float vy = rb.linearVelocity.y;
            bool isJumping = vy > 0.1f;
            bool isFalling = vy < fallThreshold;
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isFalling", isFalling);
            if (movement.x > 0f && !isFacingRight) Flip();
            else if (movement.x < 0f && isFacingRight) Flip();
        }
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
            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = baseGravityScale;
                float horizontalJump = movement.x * moveSpeed;
                rb.linearVelocity = new Vector2(horizontalJump, jumpVelocity);
                jumpBufferCounter = 0f;
                animator.SetBool("isClimbing", false);
            }
            else if (coyoteTimeCounter > 0f)
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

    private void HandleVariableGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !(jumpButton != null && jumpButton.isPressed))
        {
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
        if (isClimbing) return; // Skip wall collision checks when climbing

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

        if (!touchingWall)
            wallBoostApplied = false;

        isTouchingWall = touchingWall;

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
                if (i == wallCheckPoints.Length / 2)
                    Gizmos.DrawWireSphere(point.position, 0.05f);
            }
        }
    }

    #endregion
}