using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Move")]
    public float extraGeneralGravity;
    public float extraDecsentGravity;
    public float moveSpeed = 20;
    [HideInInspector] public Vector2 moveDir;
    [SerializeField] private float maxForceCoolDown;

    [Header("Stun")]
    private float stunTimer;

    [Header("Jump")]
    private bool canJump;
    public float jumpPower = 10;
    private bool isJumping;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPos;
    public Vector3 groundCheckSize = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Fall Speed")]
    public float maxFallSpeed = 18;

    private Coroutine appliedForceRoutine;
    private Coroutine punchingCoroutine;
    private bool isPunching;

    public void StartMovement(bool canJump)
    {
        
        this.canJump = canJump;
        rb = GetComponent<Rigidbody>();
        stunTimer = 0f;
    }

    public void UpdateMovement(out Vector2 moveDir, out bool isJumping, out bool isPunching)
    {
        GroundCheck();

        moveDir = this.moveDir;
        isJumping = false;
        isPunching = this.isPunching;
    }
    
    public void FixedUpdateMovement()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
        }

        Vector3 force = Vector3.zero;

        if(stunTimer <= 0){
            force = new Vector3(
                moveDir.x * moveSpeed * Time.deltaTime,
                0f,
                moveDir.y * moveSpeed * Time.deltaTime
            );
        }
        rb.MovePosition(transform.position + force);
        ExtraGravity();
    }

    public void ApplyForce(Vector3 force, float stunDuration, ForceMode forceMode)
    {
        if(stunDuration > 0 && stunTimer <= 0)
        {
            Stun(stunDuration);
        }

        if (appliedForceRoutine != null) return;
        rb.AddForce(force, forceMode);
        appliedForceRoutine = StartCoroutine(ApplyForceRoutine());
    }

    private IEnumerator ApplyForceRoutine()
    {
        yield return new WaitForSeconds(maxForceCoolDown);
        appliedForceRoutine = null;
        rb.linearVelocity = Vector3.zero;
    }

    private IEnumerator PunchingCoroutine()
    {
        isPunching = true;
        yield return new WaitForSeconds(.6f);
        isPunching = false;
        punchingCoroutine = null;
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

    public void Attack(InputAction.CallbackContext context)
    {
        if(context.canceled) return;
        Debug.Log("Punching");
        if (punchingCoroutine == null) { punchingCoroutine = StartCoroutine(PunchingCoroutine()); Debug.Log("Punch Started"); }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (stunTimer > 0f || isJumping) return;
        isJumping = true;
        rb.AddForce(new Vector3(0,jumpPower,0), ForceMode.Impulse);
        /*
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
        */
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

        if (hits.Length > 0 && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
        }
    }

    private void ExtraGravity()
    {
        //if (rb.linearVelocity.y >= 0) return;
        Vector3 velocity = rb.linearVelocity;
        velocity.y -= extraGeneralGravity * Time.deltaTime;
        if(velocity.y < 0) velocity.y -= extraDecsentGravity * Time.deltaTime;
        rb.linearVelocity = velocity;
        LimitFallSpeed();
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

    public void ResetMovement()
    {
        if (appliedForceRoutine != null)
        {
            StopCoroutine(appliedForceRoutine);
            appliedForceRoutine = null;
        }
        rb.linearVelocity = Vector3.zero;
        stunTimer = -1;
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