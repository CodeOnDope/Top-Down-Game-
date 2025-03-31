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
    [Range(0, 360)] public float detectionAngle = 90f; // Cone angle for detection
    public LayerMask obstacleMask; // Mask for obstacles (e.g., walls)

    [Header("Attack Parameters")]
    public float attackRadius = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float retreatThreshold = 30f;

    [Header("FOV Visualization")]
    public Color detectionRangeColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency
    public Color attackRangeColor = new Color(1f, 0.5f, 0f, 0.4f); // Orange with transparency
    public bool showAttackRadius = true;
    [Range(0.05f, 1f)]
    public float fovUpdateRate = 0.1f; // How often to update the FOV mesh (seconds)

    [Header("Visualization Rendering")]
    public string sortingLayerName = "Default"; // Set this to match your game's sorting layers
    public int fovSortingOrder = 1; // Make sure this is above background, below characters
    public int attackRadiusSortingOrder = 0; // Below FOV

    // Component references
    private Transform playerTransform;
    private Rigidbody2D rb;

    // Waypoint-related variables
    public Transform[] patrolPoints;
    private int currentWaypointIndex = 0;

    // Internal state tracking
    private float currentHealth;
    private float lastAttackTime;
    private SpriteRenderer spriteRenderer;

    // Field of View (FOV) visualization
    private Mesh viewMesh;
    private MeshRenderer viewMeshRenderer;
    private GameObject fovObject;
    private GameObject attackRadiusObject;
    private float nextFOVUpdateTime;
    private Material fovMaterial;
    private Material attackRadiusMaterial;

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

        // Create materials for the visualizations
        CreateVisualizationMaterials();

        // Initialize FOV mesh
        CreateFOVMesh();

        // Initialize attack radius visualization
        if (showAttackRadius)
        {
            CreateAttackRadiusVisualization();
        }
    }

    void CreateVisualizationMaterials()
    {
        // Create materials for FOV and attack radius
        Shader transparentShader = Shader.Find("Sprites/Default");

        if (transparentShader == null)
        {
            Debug.LogError("Could not find the Sprites/Default shader. Using fallback.");
            transparentShader = Shader.Find("Standard");
        }

        // Create FOV material
        fovMaterial = new Material(transparentShader);
        fovMaterial.color = detectionRangeColor;

        // Create attack radius material
        attackRadiusMaterial = new Material(transparentShader);
        attackRadiusMaterial.color = attackRangeColor;
    }

    void CreateFOVMesh()
    {
        // Create a new GameObject for the FOV
        fovObject = new GameObject("FOV Mesh");
        fovObject.transform.parent = transform;
        fovObject.transform.localPosition = Vector3.zero;
        fovObject.transform.localRotation = Quaternion.identity;

        // Add mesh components
        MeshFilter meshFilter = fovObject.AddComponent<MeshFilter>();
        viewMeshRenderer = fovObject.AddComponent<MeshRenderer>();
        viewMeshRenderer.material = fovMaterial;

        // Set the sorting layer to ensure it's visible in gameplay
        viewMeshRenderer.sortingLayerName = sortingLayerName;
        viewMeshRenderer.sortingOrder = fovSortingOrder;

        // Create a new mesh
        viewMesh = new Mesh();
        meshFilter.mesh = viewMesh;

        // Make sure it's visible
        viewMeshRenderer.enabled = true;
    }

    void CreateAttackRadiusVisualization()
    {
        // Create a circle visualization for attack radius
        attackRadiusObject = new GameObject("Attack Radius");
        attackRadiusObject.transform.parent = transform;
        attackRadiusObject.transform.localPosition = Vector3.zero;
        attackRadiusObject.transform.localRotation = Quaternion.identity;

        // Add mesh components
        MeshFilter meshFilter = attackRadiusObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = attackRadiusObject.AddComponent<MeshRenderer>();
        meshRenderer.material = attackRadiusMaterial;

        // Set the sorting layer
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = attackRadiusSortingOrder;

        // Create a circle mesh
        meshFilter.mesh = CreateCircleMesh(attackRadius, 32);

        // Make sure it's visible
        meshRenderer.enabled = true;
    }

    Mesh CreateCircleMesh(float radius, int segments)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * 2 * Mathf.PI;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
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

        // Update the FOV visualization (throttled for performance)
        if (Time.time >= nextFOVUpdateTime)
        {
            DrawFieldOfView();
            nextFOVUpdateTime = Time.time + fovUpdateRate;
        }

        // Update visualization colors based on state
        UpdateVisualizationColors();
    }

    void UpdateVisualizationColors()
    {
        Color currentFOVColor;

        // Change color based on enemy state
        switch (currentState)
        {
            case EnemyState.Chase:
                currentFOVColor = new Color(1f, 0.3f, 0.3f, 0.5f); // Brighter red when chasing
                break;
            case EnemyState.Attack:
                currentFOVColor = new Color(1f, 0f, 0f, 0.6f); // Intense red when attacking
                break;
            case EnemyState.Retreat:
                currentFOVColor = new Color(0.3f, 0.3f, 1f, 0.4f); // Blue when retreating
                break;
            default:
                currentFOVColor = detectionRangeColor; // Default color
                break;
        }

        // Apply the color if meshes exist
        if (viewMeshRenderer != null && viewMeshRenderer.material != null)
        {
            viewMeshRenderer.material.color = currentFOVColor;
        }
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
        if (IsPlayerInView())
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
                    currentState = EnemyState.Chase;
                    break;

                case EnemyState.Chase:
                    // Attack if player is in attack range
                    if (distanceToPlayer <= attackRadius)
                    {
                        currentState = EnemyState.Attack;
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
        else if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        {
            // Return to patrol if player is out of view
            currentState = EnemyState.Patrol;
        }
    }

    // Check if the player is within the enemy's cone of vision
    bool IsPlayerInView()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            float angleToPlayer = Vector2.Angle(transform.right, directionToPlayer);
            if (angleToPlayer <= detectionAngle / 2)
            {
                if (!Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true; // Player is in view
                }
            }
        }
        return false;
    }

    // Draw the Field of View (FOV) mesh
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(detectionAngle); // Higher resolution for better visualization
        float stepAngleSize = detectionAngle / stepCount;

        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - detectionAngle / 2 + stepAngleSize * i;
            Vector3 direction = DirFromAngle(angle, true);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionRadius, obstacleMask);

            if (hit.collider == null)
            {
                vertices[i + 1] = direction * detectionRadius;
            }
            else
            {
                vertices[i + 1] = direction * hit.distance;
            }
        }

        for (int i = 0; i < stepCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    Vector3 DirFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    // Make the enemy take damage
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        // Flash the enemy to indicate damage
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        // Destroy the enemy
        Destroy(gameObject);
    }

    // Make sure visualizations are created in editor mode too
    void OnValidate()
    {
        // Update the attack radius mesh when the attack radius changes
        if (Application.isEditor && !Application.isPlaying)
        {
            // Only update when needed
            if (attackRadiusObject != null)
            {
                MeshFilter meshFilter = attackRadiusObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = CreateCircleMesh(attackRadius, 32);
                }
            }
        }
    }

    // Visual debugging in the editor
    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw attack radius
        Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Draw patrol path
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }

        // Draw FOV in editor
        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            float halfFOVAngle = detectionAngle / 2;
            Vector3 rightDir = DirFromAngle(transform.eulerAngles.z + halfFOVAngle, true);
            Vector3 leftDir = DirFromAngle(transform.eulerAngles.z - halfFOVAngle, true);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRadius);
        }
    }
}