using UnityEngine;

public class Pickup : MonoBehaviour
{
    public PickupData pickupData; // Reference to Scriptable Object
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (pickupData != null && pickupData.pickupSprite != null)
        {
            spriteRenderer.sprite = pickupData.pickupSprite;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
           // Debug.Log($"Picked up: {pickupData.pickupName}, Value: {pickupData.value}");
            Destroy(gameObject); // Destroy the pickup
        }
    }
}