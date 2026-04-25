using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    [Header("Move")]
    public float moveSpeed = 20;
    private Vector2 moveDir;

    [Header("Jump")]
    private bool canJump;
    public float jumpPower = 10;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 1;
    public float maxFallSpeed = 18;
    public float fallSpeedMulitiplier = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartMovement(bool canJump)
    {
        this.canJump = canJump;
        rb = GetComponent<Rigidbody>();
        jumpsRemaining = maxJumps;
    }

    public void UpdateMovement()
    {
        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.y * moveSpeed);
        GroundCheck();
        Gravity();
    }

    private void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            //rb.useGravity = baseGravity * fallSpeedMulitiplier; // Fall increasingly faster
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed)); // Cap Player at Max Fallspeed
        }
        else
        {
            //rb.gravityScale = baseGravity;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("Jumps remaining " + jumpsRemaining);
        if (jumpsRemaining > 0 && canJump)
        {
            if (context.performed)
            {
                // Hold jump for full height
                Debug.Log("Jump");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpsRemaining--;
            }
            else if (context.canceled)
            {
                // Release jump early for half height
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
            }
        }
    }

    private void GroundCheck()
    {
        Debug.Log("Ground Check Start");
        foreach (var hit in Physics.OverlapBox(groundCheckPos.position,groundCheckSize))
        {
            Debug.Log("Overlap Check");
            if (hit.gameObject.layer == groundLayer)
            {
                Debug.Log("Grounded");
                jumpsRemaining = maxJumps;
            }
        }
        Debug.Log("Ground Check End");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}
