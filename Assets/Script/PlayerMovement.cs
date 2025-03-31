using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb; 
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Disable gravity for top-down movement
    }

    void Update()
    {
        // Get movement input from keyboard or controller
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down Arrow
        moveInput.Normalize(); // Prevent diagonal speed boost
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
           Destroy(gameObject);
        }
    }



}

