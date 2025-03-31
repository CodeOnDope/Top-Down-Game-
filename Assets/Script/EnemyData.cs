using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float speed;
    public int health;
    public Sprite enemySprite;
}

[CreateAssetMenu(fileName = "NewPickup", menuName = "ScriptableObjects/PickupData")]
public class PickupData : ScriptableObject
{
    public string pickupName;
    public int value;
    public Sprite pickupSprite;
}