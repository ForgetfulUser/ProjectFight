using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// HeadsUp: You need to use callbackcontext since it makes it easier to handle multiple input devices

public class NewPlayerMovement : MonoBehaviour
{
    [Header("Player Components")]
    Rigidbody rb;
    [SerializeField] GameObject groundCheckBox;
    [SerializeField] Vector3 groundCheckSize;
    [SerializeField] LayerMask groundLayer;
    [Header("Player Actions")]
 
    [Header("Player Movement")]
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float maxFallSpeed;
    bool isGrounded;
    bool isJumping;
    Vector2 movementInput;
    float yMoveVector;
    private void Start()
    {

        rb = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        groundCheck();
        
        jumpNegation();


    }
    private void FixedUpdate()
    {
      
    }

    void jumpNegation()
    {
        // Negates player velocity and adds a bit of downward force to make the jump feel less floaty
        // @@TODO: switch this to use callbackcontext 
        
        //if (!isGrounded && rb.linearVelocity.y > 0 && jump.WasReleasedThisDynamicUpdate())
        //{
        //    rb.linearVelocity = new Vector3(rb.linearVelocity.x, -1f, rb.linearVelocity.z);
        //}
    }




    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isJumping)
        {
            isJumping = true;
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }

    // @@TODO: Add section to add modifiers to change player velocity 
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>() * playerSpeed; 
        rb.linearVelocity = new Vector3(movementInput.x, rb.linearVelocity.y, movementInput.y);
    }
    void groundCheck()
    {
        Collider[] hits = Physics.OverlapBox(groundCheckBox.transform.position, groundCheckSize, transform.rotation, groundLayer);
        if (hits.Length > 0)
        {
            isGrounded = true;
            isJumping = false;
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

