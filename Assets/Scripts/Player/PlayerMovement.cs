using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Horizonal Movement")]
    private Vector2 moveDirection;
    [SerializeField] private float speed;

    [Header("Jumping")]
    private bool canJump;
    [SerializeField] private float jumpForce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartMovement(bool canJump)
    {
        this.canJump = canJump;
    }

    // Update is called once per frame
    public void UpdateMovement()
    {
        
    }

    public void Move(InputAction context)
    {

    }
}
