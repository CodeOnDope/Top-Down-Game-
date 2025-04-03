using UnityEngine;

[CreateAssetMenu(fileName = "NewPickup", menuName = "ScriptableObjects/PickupData", order = 1)]
public class PickupData : ScriptableObject
{
    [Header("Basic Info")]
    public string pickupName; // Name of the pickup
    public Sprite pickupSprite; // Sprite for the pickup

    [Header("Pickup Type")]
    public PickupType type; // Type of the pickup (e.g., Health, Ammo, PowerUp)

    [Header("Pickup Effects")]
    public int healthRestoreAmount; // Amount of health restored (if type is Health)
    public int ammoAmount; // Amount of ammo provided (if type is Ammo)
    public float powerUpDuration; // Duration of the power-up effect (if type is PowerUp)

    public enum PickupType
    {
        Health,     // Restores health
        Ammo,       // Provides ammunition
        PowerUp     // Temporary power-up (e.g., increased speed, damage)
    }
}