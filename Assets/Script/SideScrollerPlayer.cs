using UnityEngine;

public class SideScrollerPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;        // Player movement speed
    public float jumpForce = 10f;       // Force applied when the player jumps
    public Transform groundCheck;       // A reference to the ground check position (used to check if the player is grounded)
    public LayerMask groundLayer;       // The ground layer to check for collisions

    private Rigidbody2D rb;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;  // The radius for the ground check circle
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
    }

    void Update()
    {
        // Check if the player is on the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Get input for horizontal movement
        moveInput = Input.GetAxisRaw("Horizontal");

        // Make the player move
        MovePlayer();

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void MovePlayer()
    {
        // Move the player horizontally based on input (left or right)
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        // Apply a vertical force for jumping
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //GameManager.instance.EnemyDestroyed();
    }
}
