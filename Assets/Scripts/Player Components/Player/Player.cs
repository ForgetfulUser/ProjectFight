using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public PlayerAnimationController PlayerAnimationController;
    public PlayerPushComponent PlayerPushComponent;
    public PlayerCanvasManager PlayerCanvasManager;
    public InfectedPlayerAttackComponent InfectedPlayerAttackComponent;

    [Header("Player Indicator")]
    public int PlayerIndex = -1;
    //public List<GameObject> PlayerDecals = new List<GameObject>();

    [Header("Stats")]
    public int CurrentLives;
    public int MaxLives;

    [Header("Reset Variables")]
    public Vector3 StartPos;
    public bool IsActive = true;

    public virtual void StartPlayer(bool canJump, int playerIndex)
    {
        PlayerMovement.StartMovement(canJump);
        PlayerIndex = playerIndex;
        PlayerCanvasManager.StartManager(PlayerIndex);
        PlayerAnimationController.StartAnimation(PlayerIndex);
        StartPos = transform.localPosition;
    }

    public virtual void UpdatePlayer()
    {
        PlayerMovement.UpdateMovement(out Vector2 moveDir, out bool isJumping, out bool isPunching);

        if (PlayerPushComponent != null)
        {
            PlayerPushComponent.UpdatePushComponent(moveDir);
        }

        if (InfectedPlayerAttackComponent != null)
        {
            InfectedPlayerAttackComponent.UpdateAttackComponent(moveDir);
        }

        PlayerAnimationController.UpdateAnimation(moveDir, isJumping, isPunching);
    }

    public virtual void FixedUpdatePlayer()
    {
        PlayerMovement.FixedUpdateMovement();
    }

    public virtual void ResetPlayer()
    {
        transform.localPosition = StartPos;
        PlayerMovement.ResetMovement();
    }

    public virtual void TagPlayer()
    {

    }
}