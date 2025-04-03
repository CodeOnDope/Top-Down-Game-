using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName; // Name of the enemy
    public Sprite enemySprite; // Sprite for the enemy

    [Header("Stats")]
    public float speed = 2f; // Movement speed of the enemy
    public float maxHealth = 50f; // Maximum health of the enemy
    public float detectionRadius = 5f; // Radius within which the enemy detects the player
    public float attackRadius = 3f; // Radius within which the enemy attacks the player

    [Header("Shooting Settings")]
    public GameObject bulletPrefab; // Bullet prefab for the enemy
    public float bulletSpeed = 5f; // Speed of bullets fired by the enemy
    public float attackCooldown = 1.5f; // Time between attacks

    [Header("Pickup Settings")]
    public PickupData pickupToDrop; // The pickup the enemy will drop upon death
    public float dropChance = 0.5f; // Chance (0 to 1) of dropping the pickup
}