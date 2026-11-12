using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Gravity")]
    [SerializeField, Min(0.01f)] private float gravityScale = 1.25f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Audio and Hazard")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private string trapTag = "Trap";
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PhysicsMaterial2D movementMaterial;

    // Animation
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Friction against a vertical wall must not support the player's weight.
        movementMaterial = new PhysicsMaterial2D("Player No Friction")
        {
            friction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine2D.Minimum
        };
        rb.sharedMaterial = movementMaterial;
        foreach (Collider2D playerCollider in GetComponentsInChildren<Collider2D>(true))
        {
            if (playerCollider.attachedRigidbody == rb)
                playerCollider.sharedMaterial = movementMaterial;
        }
    }

    private void Update()
    {
        // Face the direction the player is moving
        if (spriteRenderer != null && moveInput.x != 0f)
        {
            spriteRenderer.flipX = moveInput.x < 0f;
        }

        // Tell the Animator what the player is doing
        if (animator != null)
        {
            animator.SetFloat(SpeedParam, Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool(IsGroundedParam, IsGrounded());
        }
    }

    private void OnDestroy()
    {
        if (movementMaterial != null)
            Destroy(movementMaterial);
    }

    private void FixedUpdate()
    {
        // Left and right movement
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );

        // Makes the player fall faster
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                rb.gravityScale *
                (fallMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    private void OCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(trapTag))
        {
            PlayDeathSound();
        }
    }

    private void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }


    // Called by the Move Input Action
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Called by the Jump Input Action
    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }
    }

    // Checks if the player is touching the ground
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }
}