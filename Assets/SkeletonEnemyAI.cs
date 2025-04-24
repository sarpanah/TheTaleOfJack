using UnityEngine;

public class SkeletonEnemyAI2D : MonoBehaviour
{
    public Transform pointA, pointB, player;
    public float patrolSpeed = 1.5f, chaseSpeed = 3.5f;
    public float detectionRange = 8f, attackRange = 1.5f;
    public Vector2 eyeOffset = new Vector2(0, 0.5f);
    public LayerMask visionMask;
    public float attackDuration = 1f;

    private enum State { Patrol, Chase, Attack }
    private State state;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private SkeletonAttackAI enemyAttack;

    private Vector2 patrolTarget;
    private float attackTimer;
    private float defaultScaleX;

    private static readonly int HASH_SPEED  = Animator.StringToHash("Speed");
    private static readonly int HASH_ATTACK = Animator.StringToHash("Attack");

    void Awake()
    {
        // Core components
        rb          = GetComponent<Rigidbody2D>();
        anim        = GetComponent<Animator>();
        sprite      = GetComponent<SpriteRenderer>();
        enemyAttack = GetComponent<SkeletonAttackAI>();

        // Capture the default X scale for flipping
        defaultScaleX = transform.localScale.x;

        if (!pointA || !pointB || !player)
        {
            Debug.LogError("PointA, PointB, or Player not assigned!", this);
            enabled = false;
            return;
        }

        patrolTarget = pointB.position;
        state        = State.Patrol;
    }

    void Update()
    {
        // If we're attacking, update timer and exit
        if (state == State.Attack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                EndAttack();

            anim.SetFloat(HASH_SPEED, 0f);
            return;
        }

        // Check for player visibility
        if (CanSeePlayer())
        {
            state = State.Chase;
        }

        // Transition to attack if in range
        float sqrDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        if (state == State.Chase && sqrDist <= attackRange * attackRange)
        {
            state       = State.Attack;
            attackTimer = attackDuration;
            enemyAttack.Attack();
            return;
        }

        // Lost sight: resume patrol
        if (state == State.Chase && !CanSeePlayer())
        {
            state = State.Patrol;
            float distA = Vector2.Distance(transform.position, pointA.position);
            float distB = Vector2.Distance(transform.position, pointB.position);
            patrolTarget = (distA < distB) ? pointA.position : pointB.position;
        }

        // Update animation blend based on speed
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat(HASH_SPEED, currentSpeed / chaseSpeed);
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
    }

    private bool CanSeePlayer()
    {
        if(player==null){
            return false;
        }
        Vector2 eyePos = (Vector2)transform.position + eyeOffset;
        Vector2 dir    = ((Vector2)player.position - eyePos).normalized;
        RaycastHit2D hit = Physics2D.Raycast(eyePos, dir, detectionRange, visionMask);

        bool canSee = (hit.collider != null && hit.collider.transform == player);
        Debug.DrawRay(eyePos, dir * detectionRange, canSee ? Color.green : Color.red);
        return canSee;
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
        if (pointA && pointB)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (patrolSpeed < 0)  patrolSpeed = 0;
        if (chaseSpeed  < 0)  chaseSpeed  = 0;
        if (detectionRange < 0) detectionRange = 0;
        if (attackRange    < 0) attackRange    = 0;
        if (attackDuration < 0) attackDuration = 0;
    }
#endif
}
