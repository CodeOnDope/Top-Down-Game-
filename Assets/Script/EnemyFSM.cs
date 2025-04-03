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
    public EnemyData enemyData; // Reference to EnemyData ScriptableObject

    // Movement and detection parameters
    [Header("Movement Settings")]
    public Transform[] patrolPoints; // Waypoints for patrolling
    private int currentWaypointIndex = 0;

    // Component references
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Internal state tracking
    private float currentHealth;
    private float lastAttackTime;

    void Start()
    {
        // Initialize state and references
        currentState = EnemyState.Patrol;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Initialize stats from EnemyData
        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;

            // Set sprite
            if (enemyData.enemySprite != null)
            {
                spriteRenderer.sprite = enemyData.enemySprite;
            }
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
        if (patrolPoints.Length == 0)
        {
            currentState = EnemyState.Idle;
            return;
        }

        Transform targetPoint = patrolPoints[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, enemyData.speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
        }
    }

    // Chase player when detected
    void PerformChase()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, enemyData.speed * Time.deltaTime);
    }

    // Attack player when in range
    void PerformAttack()
    {
        if (Time.time - lastAttackTime > enemyData.attackCooldown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }
    }

    void Shoot()
    {
        if (enemyData.bulletPrefab == null || playerTransform == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(enemyData.bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * enemyData.bulletSpeed;
        }

        Destroy(bullet, 5f);
    }

    // Retreat when health is low
    void PerformRetreat()
    {
        Vector2 retreatDirection = (transform.position - playerTransform.position).normalized;

        // Ensure no excessive force is applied
        rb.linearVelocity = retreatDirection * enemyData.speed;

        // Reset velocity if retreating too fast
        if (rb.linearVelocity.magnitude > enemyData.speed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * enemyData.speed;
        }
    }

    // Idle state
    void PerformIdle()
    {
        rb.linearVelocity = Vector2.zero;
    }

    // State transition logic
    void CheckStateTransitions()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (currentHealth <= enemyData.maxHealth * 0.1f) // Retreat if health is below 10%
        {
            currentState = EnemyState.Retreat;
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distanceToPlayer <= enemyData.detectionRadius)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= enemyData.attackRadius)
                {
                    currentState = EnemyState.Attack;
                }
                else if (distanceToPlayer > enemyData.detectionRadius)
                {
                    currentState = EnemyState.Patrol;
                }
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > enemyData.attackRadius)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Retreat:
                if (currentHealth > enemyData.maxHealth * 0.1f)
                {
                    currentState = EnemyState.Patrol;
                }
                break;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Reset velocity to prevent flying away
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Freeze position if health is low
            if (currentHealth <= enemyData.maxHealth * 0.1f) // 10% health threshold
            {
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.None;
            }
        }

        Debug.Log($"Enemy Health: {currentHealth}, Velocity: {rb.linearVelocity}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (enemyData != null && enemyData.pickupToDrop != null)
        {
            float randomValue = Random.value;
            if (randomValue <= enemyData.dropChance)
            {
                DropPickup();
            }
        }

        Destroy(gameObject);
    }

    void DropPickup()
    {
        GameObject pickup = new GameObject("Pickup");
        pickup.transform.position = transform.position;

        SpriteRenderer pickupSpriteRenderer = pickup.AddComponent<SpriteRenderer>();
        pickupSpriteRenderer.sprite = enemyData.pickupToDrop.pickupSprite;

        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        Pickup pickupScript = pickup.AddComponent<Pickup>();
        pickupScript.pickupData = enemyData.pickupToDrop;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRadius);
    }
}