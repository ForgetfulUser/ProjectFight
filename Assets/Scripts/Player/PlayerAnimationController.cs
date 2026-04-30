using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator Animator;
    private Sprite jumpSprite;
    public SpriteRenderer SpriteRenderer;
    private bool lastJumpVal;
    public void StartAnimation(int playerIndex)
    {
        GameObject decal = null;
        switch (playerIndex)
        {
            default:
                Animator.SetBool("Is Player One", true);
                decal = Resources.Load<GameObject>("Decals/P1 Blob");
                break;
            case 1:
                Animator.SetBool("Is Player Two", true);
                decal = Resources.Load<GameObject>("Decals/P2 Blob");
                break;
            case 2:
                Animator.SetBool("Is Player Three", true);
                decal = Resources.Load<GameObject>("Decals/P3 Blob");
                break;
            case 3:
                Animator.SetBool("Is Player Four", true);
                decal = Resources.Load<GameObject>("Decals/P4 Blob");
                break;
        }

        if (decal != null && playerIndex >= 0 && playerIndex < 4)
        {
            decal = Instantiate(decal, transform);
        }
        else
        {
            Debug.LogWarning("Player Index: " + playerIndex + " is out of range.");
        }
    }

    public void UpdateAnimation(Vector2 moveDir, bool isJumping, bool isPunching)
    {
        Animator.SetInteger("Horizontal Movement", Mathf.RoundToInt(moveDir.x));
        Animator.SetInteger("Vertical Movement", Mathf.RoundToInt(moveDir.y));
        Animator.SetBool("Is Punching", isPunching);
        SpriteRenderer.flipX = (isPunching && moveDir.x > 0);
        //if(isJumping != lastJumpVal) ToggleJump();
    }

    private void ToggleJump()
    {
        lastJumpVal = !lastJumpVal;
        if(lastJumpVal == true)
        {
            Animator.StopPlayback();
            GetComponent<SpriteRenderer>().sprite = jumpSprite;
        }
        else
        {
            Animator.StartPlayback();
        }
    }
}
