using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public PlayerAnimationController PlayerAnimationController;
    public PlayerPushComponent PlayerPushComponent;

    [Header("Player Indicator")]
    public int PlayerIndex = -1;
    public List<GameObject> PlayerDecals = new List<GameObject>();

    [Header("Stats")]
    public int CurrentLives;
    public int MaxLives;

    [Header("Reset Variables")]
    public Vector3 StartPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void StartPlayer(bool canJump, int playerIndex)
    {
        PlayerMovement.StartMovement(canJump);
        PlayerIndex = playerIndex;
        PlayerDecals[PlayerIndex].SetActive(true);
        PlayerAnimationController.StartAnimation(PlayerIndex);
        StartPos = transform.localPosition;
    }

    // Update is called once per frame
    public virtual void UpdatePlayer()
    {
        PlayerMovement.UpdateMovement(out Vector2 moveDir, out bool isJumping);
        PlayerAnimationController.UpdateAnimation(moveDir, isJumping);
        PlayerPushComponent.UpdatePushComponent(moveDir);
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
