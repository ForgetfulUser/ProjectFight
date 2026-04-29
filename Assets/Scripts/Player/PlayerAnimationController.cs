using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator Animator;
    private Sprite jumpSprite;
    private bool lastJumpVal;
    public void StartAnimation(int playerIndex)
    {
        GameObject decal = null;
        switch (playerIndex)
        {
            case 0:
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

        if (decal != null)
        {
            decal = Instantiate(decal, transform);
        }
        else
        {
            Debug.LogError("Player Index: " + playerIndex + " is out of range.");
        }
    }

    public void UpdateAnimation(Vector2 moveDir, bool isJumping)
    {
        Animator.SetInteger("Horizontal Movement", Mathf.RoundToInt(moveDir.x));
        Animator.SetInteger("Vertical Movement", Mathf.RoundToInt(moveDir.y));

        if(isJumping != lastJumpVal) ToggleJump();
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
