using UnityEngine;

public class TestPlayerMovementRunner : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public bool canJump = true;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }

    private void Start()
    {
        if (playerMovement != null)
        {
            playerMovement.StartMovement(canJump);
        }
    }

    private void Update()
    {
        if (playerMovement == null)
        {
            return;
        }

        Vector2 currentMoveDir;
        playerMovement.UpdateMovement(out currentMoveDir, out bool isJumping);
    }

    private void FixedUpdate()
    {
        if (playerMovement == null)
        {
            return;
        }

        //playerMovement.UpdateFixedMovement();
    }
}