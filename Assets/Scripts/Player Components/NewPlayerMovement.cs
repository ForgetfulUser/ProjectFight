using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour
{
    [Header("Player Components")]
    Rigidbody rb;
    [SerializeField] GameObject groundCheckBox;
    [SerializeField] Vector3 groundCheckSize;
    [SerializeField] LayerMask groundLayer;
    [Header("Player Actions")]
    InputAction move;
    InputAction jump;

    [Header("Player Movement")]
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpForce;
    bool isGrounded;
    bool isJumping;
    Vector2 xzMoveVector;
    float yMoveVector;
    private void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        jump = InputSystem.actions.FindAction("Jump");
        rb = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        groundCheck();


        if (jump.WasPerformedThisFrame())
        {
            if (isGrounded) 
            {
                playerJump();
            }
            // Negates velocity to give more precise jump control
        }
        if (!isGrounded && jump.WasReleasedThisFrame())
        {
             rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }

    }

    IEnumerator jumpCooldown() // might deprecate this
    {
        yield return new WaitForSeconds(2.0f);
        isJumping = false;
    }


    Vector3 updatePlayerMovement()
    {
        Vector3 finalMoveVector;
        xzMoveVector = move.ReadValue<Vector2>();
        xzMoveVector = xzMoveVector * playerSpeed;
        return finalMoveVector = new Vector3(xzMoveVector.x, yMoveVector, xzMoveVector.y);
    }
    void playerJump()
    {
        isJumping = true;
        rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    }

    void groundCheck()
    {
        Collider[] hits = Physics.OverlapBox(groundCheckBox.transform.position, groundCheckSize, transform.rotation, groundLayer);
        if (hits.Length > 0)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(groundCheckBox.transform.position, groundCheckSize);
    }
}

