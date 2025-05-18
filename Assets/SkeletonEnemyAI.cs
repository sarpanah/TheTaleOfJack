using UnityEngine;

public class SkeletonEnemyAI2D : MonoBehaviour
{
    public Transform pointA, pointB, player;
    public float patrolSpeed = 1.5f, chaseSpeed = 3.5f;
    public float detectionRange = 8f, attackRange = 1.5f, chaseRange = 3f;
    public float idleDuration = 2f; // Cooldown period in seconds
    public Vector2 eyeOffset = new Vector2(0, 0.5f);
    public LayerMask visionMask;
    public float attackDuration = 1f;

    private enum State { Patrol, Chase, Attack, Idle }
    private State state;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private SkeletonAttackAI enemyAttack;

    private Vector2 patrolTarget;
    private float attackTimer;
    private float idleTimer; // Timer for idle state cooldown
    private float defaultScaleX;

    private static readonly int HASH_SPEED = Animator.StringToHash("Speed");
    private static readonly int HASH_ATTACK = Animator.StringToHash("Attack");

    // Add this field
    [SerializeField] private EnemyHUDController hudController;
    void Awake()
    {
        // Core components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        enemyAttack = GetComponent<SkeletonAttackAI>();
         if (hudController == null)
            hudController = GetComponentInChildren<EnemyHUDController>();

        // Capture the default X scale for flipping
        defaultScaleX = transform.localScale.x;

        if (!pointA || !pointB || !player)
        {
            Debug.LogError("PointA, PointB, or Player not assigned!", this);
            enabled = false;
            return;
        }

        patrolTarget = pointB.position;
        state = State.Patrol;
    }

    void Update()
    {
        // Handle Attack state
        if (state == State.Attack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                EndAttack();
            anim.SetFloat(HASH_SPEED, 0f);
            return;
        }

        bool canSeePlayer = CanSeePlayer();

        // Transition from Patrol to Chase
        if (state == State.Patrol && canSeePlayer)
        {
            state = State.Chase;
        }

        if (state == State.Chase)
        {
            float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
            // Transition to Attack if within range
            if (sqrDist <= attackRange * attackRange)
            {
                state = State.Attack;
                attackTimer = attackDuration;
                enemyAttack.Attack();
            }
            // Transition to Idle if player is too far
            else if (sqrDist > chaseRange * chaseRange)
            {
                state = State.Idle;
                idleTimer = idleDuration;
            }
        }
        else if (state == State.Idle)
        {
            idleTimer -= Time.deltaTime;
            float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
            // Resume Chase if player is visible and within chase range
            if (canSeePlayer && sqrDist <= chaseRange * chaseRange)
            {
                state = State.Chase;
            }
            // Return to Patrol after cooldown if no chase condition met
            else if (idleTimer <= 0f)
            {
                state = State.Patrol;
                float distA = Vector2.Distance(transform.position, pointA.position);
                float distB = Vector2.Distance(transform.position, pointB.position);
                patrolTarget = (distA < distB) ? pointA.position : pointB.position;
            }
        }

        // Update animation
        if (state == State.Attack || state == State.Idle)
        {
            anim.SetFloat(HASH_SPEED, 0f);
        }
        else
        {
            float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat(HASH_SPEED, currentSpeed / chaseSpeed);
        }
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                DoPatrol();
                break;
            case State.Chase:
                MoveTowards(player.position, chaseSpeed);
                break;
            case State.Attack:
            case State.Idle:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    private void DoPatrol()
    {
        Vector2 dirVec = patrolTarget - (Vector2)transform.position;
        float dir = Mathf.Sign(dirVec.x);
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);

        // Flip using scale.x
        FlipScaleX(dir);

        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.1f)
            patrolTarget = (patrolTarget == (Vector2)pointA.position) ? pointB.position : pointA.position;
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 delta = target - (Vector2)transform.position;
        delta.y = 0f;
        if (delta.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        delta.Normalize();
        rb.linearVelocity = new Vector2(delta.x * speed, rb.linearVelocity.y);

        // Flip using scale.x
        FlipScaleX(Mathf.Sign(delta.x));
    }

    private void FlipScaleX(float dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = defaultScaleX * dir;
        transform.localScale = scale;

        // Flip HUD to match enemy direction
        if (hudController != null)
            hudController.MatchParentDirection(dir);
    }

    private bool CanSeePlayer()
{
    if (player == null)
    {
        return false;
    }

    // Calculate squared distance to player for efficiency
    Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
    float sqrDist = toPlayer.sqrMagnitude;
    if (sqrDist > detectionRange * detectionRange)
    {
        return false; // Player is too far, no need for raycasts
    }

    // Determine facing direction based on enemy's scale (positive x = right, negative x = left)
    Vector2 eyePos = (Vector2)transform.position + eyeOffset;
    Vector2 forwardDir = new Vector2(Mathf.Sign(transform.localScale.x), 0);
    Vector2 backwardDir = -forwardDir;

    // Calculate backward detection range
    float backwardDetectionRange = detectionRange / 2f;

    // Cast ray in forward direction
    RaycastHit2D forwardHit = Physics2D.Raycast(eyePos, forwardDir, detectionRange, visionMask);
    bool forwardCanSee = (forwardHit.collider != null && forwardHit.collider.transform == player);

    // Cast ray in backward direction with half the range
    RaycastHit2D backwardHit = Physics2D.Raycast(eyePos, backwardDir, backwardDetectionRange, visionMask);
    bool backwardCanSee = (backwardHit.collider != null && backwardHit.collider.transform == player);

    // Visualize both rays for debugging
    Debug.DrawRay(eyePos, forwardDir * detectionRange, forwardCanSee ? Color.green : Color.red);
    Debug.DrawRay(eyePos, backwardDir * backwardDetectionRange, backwardCanSee ? Color.green : Color.red);

    // Return true if player is seen in either direction
    return forwardCanSee || backwardCanSee;
}
    public void EndAttack()
    {
        state = CanSeePlayer() ? State.Chase : State.Patrol;

        if (state == State.Patrol)
        {
            float distA = Vector2.Distance(transform.position, pointA.position);
            float distB = Vector2.Distance(transform.position, pointB.position);
            patrolTarget = (distA < distB) ? pointA.position : pointB.position;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)eyeOffset, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        if (pointA && pointB)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (patrolSpeed < 0) patrolSpeed = 0;
        if (chaseSpeed < 0) chaseSpeed = 0;
        if (detectionRange < 0) detectionRange = 0;
        if (attackRange < 0) attackRange = 0;
        if (chaseRange < 0) chaseRange = 0;
        if (idleDuration < 0) idleDuration = 0;
        if (attackDuration < 0) attackDuration = 0;
    }
#endif
}