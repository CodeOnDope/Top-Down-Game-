using UnityEngine;

// Enum to define possible enemy states
public enum EnemyState
{
    Patrol,      // Moving between waypoints
    Chase,       // Pursuing the player
    Attack,      // Attacking the player
    Retreat,     // Running away when low on health
    Idle         // Stationary state
}

// Main FSM controller for enemy AI
public class EnemyFSM : MonoBehaviour
{
    // State-related variables
    private EnemyState currentState;
    public EnemyData enemyData;

    // Movement and detection parameters
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float retreatSpeed = 2.5f;

    [Header("Detection Parameters")]
    public float detectionRadius = 5f;
    public float attackRadius = 1.5f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float retreatThreshold = 30f;

    // Component references
    private Transform playerTransform;
    private Rigidbody2D rb;

    // Waypoint-related variables
    public Transform[] patrolPoints;
    private int currentWaypointIndex = 0;

    // Internal state tracking
    private float currentHealth;
    private float lastAttackTime;
    public float attackCooldown = 1.5f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Initialize state and references
        currentState = EnemyState.Patrol;
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (enemyData != null && enemyData.enemySprite != null)
        {
            spriteRenderer.sprite = enemyData.enemySprite;
        }
    }

    void Update()
    {
        // State machine logic
        switch (currentState)
        {
            case EnemyState.Patrol:
                PerformPatrol();
                break;
            case EnemyState.Chase:
                PerformChase();
                break;
            case EnemyState.Attack:
                PerformAttack();
                break;
            case EnemyState.Retreat:
                PerformRetreat();
                break;
            case EnemyState.Idle:
                PerformIdle();
                break;
        }

        // Check for state transitions
        CheckStateTransitions();
    }

    // Patrol behavior: move between waypoints
    void PerformPatrol()
    {
        // If no waypoints, stay idle
        if (patrolPoints.Length == 0)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Move towards current waypoint
        Vector2 targetPosition = patrolPoints[currentWaypointIndex].position;
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            patrolSpeed * Time.deltaTime
        );

        // Waypoint reached, move to next
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
        }
    }

    // Chase player when detected
    void PerformChase()
    {
        // Move directly towards player
        transform.position = Vector2.MoveTowards(
            transform.position,
            playerTransform.position,
            chaseSpeed * Time.deltaTime
        );
    }

    // Attack player when in range
    void PerformAttack()
    {
        // Check if enough time has passed since last attack
        if (Time.time - lastAttackTime > attackCooldown)
        {
            // Perform attack logic
            Debug.Log("Enemy Attacked Player!");
            lastAttackTime = Time.time;
        }
    }

    // Retreat when health is low
    void PerformRetreat()
    {
        // Move away from player
        Vector2 retreatDirection = (transform.position - playerTransform.position).normalized;
        transform.position += (Vector3)retreatDirection * retreatSpeed * Time.deltaTime;
    }

    // Idle state
    void PerformIdle()
    {
        // Optional: Add idle animation or behavior
        rb.linearVelocity = Vector2.zero;
    }

    // State transition logic
    void CheckStateTransitions()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Health-based state check
        if (currentHealth <= retreatThreshold)
        {
            currentState = EnemyState.Retreat;
            return;
        }

        // State transition logic
        switch (currentState)
        {
            case EnemyState.Patrol:
                // Transition to Chase if player is detected
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                // Attack if player is in attack range
                if (distanceToPlayer <= attackRadius)
                {
                    currentState = EnemyState.Attack;
                }
                // Return to patrol if player is out of detection range
                else if (distanceToPlayer > detectionRadius)
                {
                    currentState = EnemyState.Patrol;
                }
                break;

            case EnemyState.Attack:
                // Return to chase if player moves out of attack range
                if (distanceToPlayer > attackRadius)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Retreat:
                // Return to patrol if health is restored
                if (currentHealth > retreatThreshold)
                {
                    currentState = EnemyState.Patrol;
                }
                break;
        }
    }

    // Method to take damage
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        // Optional: Add hit reaction or sound
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Enemy death method
    void Die()
    {
        // Add death logic: animation, sound, drop items
        Destroy(gameObject);
    }

    // Visualize detection and attack ranges in scene view
    void OnDrawGizmosSelected()
    {
        //Detection radius visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Attack radius visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);


    }
}