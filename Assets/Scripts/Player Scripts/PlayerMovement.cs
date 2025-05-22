using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float deceleration = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Gravity Settings")]
    [SerializeField] private float baseGravityScale = 3f;
    [SerializeField] private float fallMultiplier = 2.5f; // Consider reducing to 1.5f or 2f for smoother fall
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform[] wallCheckPoints;
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Boost Settings")]
    [SerializeField] private float wallBoostVelocity = 1f;

    [Header("Touch Controls")]
    [SerializeField] private TouchButton moveLeftButton;
    [SerializeField] private TouchButton moveRightButton;
    [SerializeField] private TouchButton jumpButton;
    [SerializeField] private TouchButton moveUpButton;
    [SerializeField] private TouchButton moveDownButton;

    [Header("Climbing Settings")]
    [SerializeField] private float climbSpeed = 3f;

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
    private bool wasGrounded;
    private float timeSinceJump = 0f; // Tracks time since last jump to delay fall transition

    #endregion

    #region Unity Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = baseGravityScale;
    }

    private void Update()
    {
        if (!isClimbing && canClimb && movement.x != 0f)
        {
            if ((movement.x > 0f && wallDirection.x > 0f) || (movement.x < 0f && wallDirection.x < 0f))
            {
                StartClimbing();
            }
        }

        UpdateGroundedState();

        // Increment timeSinceJump when not grounded
        if (!isGrounded)
        {
            timeSinceJump += Time.deltaTime;
        }
        else
        {
            timeSinceJump = 0f;
        }

        UpdateWallCollision();
        HandleJumpTiming();
        HandleJumping();
        HandleInput();

        if (isClimbing && isGrounded)
        {
            StopClimbing();
        }

        HandleAnimationAndFlipping();
        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        if (!isControlEnabled) return;

        if (rb.linearVelocity.y < -0.5f && !isClimbing)
            fallTime += Time.fixedDeltaTime;
        else
            fallTime = 0f;

        ApplyMovement();
        
        if (!isClimbing) 
            HandleVariableGravity();
    }

    #endregion

    #region Collision Detection

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ClimbableWall"))
        {
            canClimb = true;
            CalculateWallDirection(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ClimbableWall"))
        {
            canClimb = false;
            if (isClimbing) StopClimbing();
        }
    }

    #endregion

    #region Movement Logic

    private void HandleInput()
    {
        if (!moveLeftButton || !moveRightButton || !moveUpButton || !moveDownButton) return;

        movement.x = moveLeftButton.isPressed ? -1f : moveRightButton.isPressed ? 1f : 0f;
        movement.y = moveUpButton.isPressed ? 1f : moveDownButton.isPressed ? -1f : 0f;

        if (isTouchingWall && MovingTowardsWall() && !isClimbing)
            movement.x = 0f;
    }

    private void ApplyMovement()
    {
        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0f, climbSpeed);
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
            rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime), rb.linearVelocity.y);
        }
    }

    #endregion

    #region Animation & Visuals

    private void HandleAnimationAndFlipping()
    {
        if (isClimbing)
        {
            animator.SetBool("isClimbing", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else
        {
            animator.SetBool("isClimbing", false);
            
            // Horizontal movement
            bool isRunning = Mathf.Abs(movement.x) > 0.1f;
            animator.SetBool("isRunning", isRunning);

            // Vertical states
            float vy = rb.linearVelocity.y;
            bool isJumping = vy > 1f;
            bool isFalling = !isGrounded && vy < -1f && timeSinceJump > 0.1f; // Delay fall transition

            // Landing detection
            if (!wasGrounded && isGrounded)
            {
                animator.SetTrigger("Land");
                isJumping = false;
                isFalling = false;
            }

            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isFalling", isFalling);

            // Character flipping
            if (movement.x > 0.1f && !isFacingRight) Flip();
            else if (movement.x < -0.1f && isFacingRight) Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpButton && jumpButton.isPressed)
        {
            if (jumpButtonReleased)
            {
                jumpBufferCounter = jumpBufferTime;
                jumpButtonHeld = true;
                jumpButtonReleased = false;
            }
        }
        else
        {
            jumpButtonReleased = true;
            jumpButtonHeld = false;
        }

        jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJumping()
    {
        if (jumpBufferCounter > 0f)
        {
            if (isClimbing)
            {
                WallJump();
            }
            else if (coyoteTimeCounter > 0f)
            {
                Jump(false);
                jumpBufferCounter = 0f;
            }
            else if (canDoubleJump)
            {
                Jump(true);
                canDoubleJump = false;
                jumpBufferCounter = 0f;
            }
        }
    }

    private void Jump(bool isDoubleJump)
    {
        float jumpPower = isDoubleJump ? jumpVelocity * 0.8f : jumpVelocity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        animator.SetBool("isJumping", true);
        animator.SetBool("isFalling", false);
        timeSinceJump = 0f; // Reset on jump
    }

    private void WallJump()
    {
        isClimbing = false;
        rb.gravityScale = baseGravityScale;
        float horizontalJump = movement.x * moveSpeed;
        rb.linearVelocity = new Vector2(horizontalJump, jumpVelocity);
        animator.SetBool("isClimbing", false);
        canDoubleJump = true; // Enable double jump after wall jump
        timeSinceJump = 0f; // Reset on wall jump
    }

    #endregion

    #region Gravity & Physics

    private void HandleVariableGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            if (!isGrounded)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
                animator.SetBool("isFalling", true);
            }
        }
        else if (rb.linearVelocity.y > 0f && !jumpButtonHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Ground & Wall Detection

    private void UpdateGroundedState()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("IsGrounded", isGrounded);
    }

    private void UpdateWallCollision()
    {
        if (isClimbing) return;

        bool touchingWall = false;
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        foreach (Transform point in wallCheckPoints)
        {
            if (!point) continue;
            if (Physics2D.Raycast(point.position, dir, wallCheckDistance, wallLayer))
            {
                touchingWall = true;
                break;
            }
        }

        isTouchingWall = touchingWall;
    }

    #endregion

    #region Climbing System

    private void StartClimbing()
    {
        isClimbing = true;
        rb.gravityScale = 6.5f;
        animator.SetBool("isClimbing", true);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = baseGravityScale;
        animator.SetBool("isClimbing", false);
    }

    private void CalculateWallDirection(Collider2D wall)
    {
        Vector2 playerPos = transform.position;
        Vector2 wallCenter = wall.bounds.center;
        float wallLeft = wallCenter.x - wall.bounds.extents.x;
        float wallRight = wallCenter.x + wall.bounds.extents.x;

        wallDirection = playerPos.x < wallLeft ? Vector2.right : 
                       playerPos.x > wallRight ? Vector2.left : 
                       movement.x > 0 ? Vector2.right : Vector2.left;
    }

    private bool MovingTowardsWall()
    {
        return (movement.x > 0f && isFacingRight) || (movement.x < 0f && !isFacingRight);
    }

    #endregion

    #region Control Management

    public void DisableControl() => isControlEnabled = false;
    public void EnableControl() => isControlEnabled = true;
    public void FreezePlayer() => rb.constraints = RigidbodyConstraints2D.FreezeAll;

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.yellow;
        Vector2 dir = Application.isPlaying && isFacingRight ? Vector2.right : Vector2.left;
        foreach (Transform point in wallCheckPoints)
        {
            if (!point) continue;
            Gizmos.DrawLine(point.position, point.position + (Vector3)dir * wallCheckDistance);
        }
    }

    #endregion
}