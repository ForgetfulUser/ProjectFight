using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Move")]
    public float moveSpeed = 20;
    [HideInInspector] public Vector2 moveDir;

    [Header("External Push")]
    public float externalVelocityDecay = 10f;
    public float maxExternalSpeed = 24f;

    private Vector3 externalVelocity;

    [Header("Stun")]
    private float stunTimer;

    [Header("Jump")]
    private bool canJump;
    public float jumpPower = 10;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPos;
    public Vector3 groundCheckSize = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Fall Speed")]
    public float maxFallSpeed = 18;

    public void StartMovement(bool canJump)
    {
        this.canJump = canJump;
        rb = GetComponent<Rigidbody>();
        jumpsRemaining = maxJumps;

        externalVelocity = Vector3.zero;
        stunTimer = 0f;
    }

    public void UpdateMovement(out Vector2 moveDir)
    {
        GroundCheck();
        moveDir = this.moveDir;
    }

    public void UpdateFixedMovement()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
        }

        Vector3 inputVelocity = Vector3.zero;

        if (stunTimer <= 0f)
        {
            inputVelocity = new Vector3(
                moveDir.x * moveSpeed,
                0f,
                moveDir.y * moveSpeed
            );
        }

        Vector3 finalHorizontalVelocity = inputVelocity + externalVelocity;

        rb.linearVelocity = new Vector3(
            finalHorizontalVelocity.x,
            rb.linearVelocity.y,
            finalHorizontalVelocity.z
        );

        externalVelocity = Vector3.MoveTowards(
            externalVelocity,
            Vector3.zero,
            externalVelocityDecay * Time.fixedDeltaTime
        );

        LimitFallSpeed();
    }

    public void AddExternalVelocity(Vector3 velocityChange)
    {
        velocityChange.y = 0f;

        externalVelocity += velocityChange;

        Vector3 horizontalExternalVelocity = new Vector3(
            externalVelocity.x,
            0f,
            externalVelocity.z
        );

        if (horizontalExternalVelocity.magnitude > maxExternalSpeed)
        {
            horizontalExternalVelocity = horizontalExternalVelocity.normalized * maxExternalSpeed;

            externalVelocity = new Vector3(
                horizontalExternalVelocity.x,
                0f,
                horizontalExternalVelocity.z
            );
        }
    }

    public void AddVerticalVelocity(float verticalVelocity)
    {
        if (rb == null)
        {
            return;
        }

        if (verticalVelocity <= 0f)
        {
            return;
        }

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            Mathf.Max(rb.linearVelocity.y, verticalVelocity),
            rb.linearVelocity.z
        );
    }

    public void ClearExternalVelocity()
    {
        externalVelocity = Vector3.zero;
    }

    public void Stun(float duration)
    {
        if (duration > stunTimer)
        {
            stunTimer = duration;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (stunTimer > 0f)
        {
            return;
        }

        if (jumpsRemaining > 0 && canJump)
        {
            if (context.performed)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    jumpPower,
                    rb.linearVelocity.z
                );

                jumpsRemaining--;
            }
            else if (context.canceled)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * 0.5f,
                    rb.linearVelocity.z
                );

                jumpsRemaining--;
            }
        }
    }

    private void GroundCheck()
    {
        if (groundCheckPos == null)
        {
            return;
        }

        Collider[] hits = Physics.OverlapBox(
            groundCheckPos.position,
            groundCheckSize,
            Quaternion.identity,
            groundLayer
        );

        if (hits.Length > 0)
        {
            jumpsRemaining = maxJumps;
        }
    }

    private void LimitFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                -maxFallSpeed,
                rb.linearVelocity.z
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPos == null)
        {
            return;
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize * 2f);
    }
}