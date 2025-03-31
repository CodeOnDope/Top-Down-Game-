using UnityEngine;

public class DestroyProps : MonoBehaviour
{


    void OnTriggerEnter2D(Collider2D col)
    {
        // Check if the collider is the player
        if (col.gameObject.CompareTag("Player"))
        {
          Destroy(gameObject);
        }
    }

}
