using UnityEngine;

public class SideScrollerPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float accelerationForce = 50f;  // Force applied to accelerate player

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // State variables
    private bool isGrounded;
    private float moveInput;
    private bool isFacingRight = true;

    void Start()
    {
        // Get references to components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get player input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Update animations
        UpdateAnimation();

        // Update facing direction
        UpdateSpriteDirection();
    }

    void FixedUpdate()
    {
        // Check if the player is on the ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Move the player (in FixedUpdate for physics)
        MovePlayer();
    }

    void MovePlayer()
    {
        // Calculate target velocity
        float targetVelocityX = moveInput * moveSpeed;

        // Calculate how far we are from target velocity
        float velocityDiff = targetVelocityX - rb.linearVelocity.x;

        // Apply force proportional to the difference
        float forceToApply = velocityDiff * accelerationForce;

        // Apply horizontal force
        rb.AddForce(new Vector2(forceToApply, 0), ForceMode2D.Force);

        // Optional: Clamp maximum horizontal speed
        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * moveSpeed, rb.linearVelocity.y);
        }
    }

    void Jump()
    {
        // Reset vertical velocity before adding jump force for consistent jumps
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Apply a vertical impulse force for jumping
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Trigger jump animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }

    // Update sprite direction based on movement
    void UpdateSpriteDirection()
    {
        // Flip the sprite based on movement direction
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    // Flip the sprite direction
    void Flip()
    {
        isFacingRight = !isFacingRight;

        // Flip using sprite renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
        // Alternative: Flip using transform scale
        else
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
    }

    // Update animation parameters
    void UpdateAnimation()
    {
        if (animator == null) return;

        // Set animation parameters
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", isGrounded);

        // Fall animation
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // For visualization in editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // Draw ground check radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // Public method to apply damage to player (can be called by enemies)
    public void TakeDamage(float damageAmount)
    {
        // Trigger hit animation
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // Add your health logic here
        // PlayerHealth.instance.TakeDamage(damageAmount);
    }
}