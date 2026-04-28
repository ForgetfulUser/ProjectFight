using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public PlayerAnimationController PlayerAnimationController;
    public PlayerPushComponent PlayerPushComponent;
    public int PlayerIndex = -1;
    [Header("Stats")]
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentStamina;
    public int MaxStamina;
    public Vector3 StartPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartPlayer(bool canJump, int playerIndex)
    {
        PlayerMovement.StartMovement(canJump);
        PlayerIndex = playerIndex;
        StartPos = transform.localPosition;
    }

    // Update is called once per frame
    public void UpdatePlayer()
    {
        PlayerMovement.UpdateMovement(out Vector2 moveDir);
        PlayerAnimationController.UpdateAnimation(moveDir);
        PlayerPushComponent.UpdatePushComponent(moveDir);
    }

    public void FixedUpdatePlayer()
    {
        PlayerMovement.UpdateFixedMovement();
    }

    public void ResetPlayer()
    {
        transform.localPosition = StartPos;
    }
}
