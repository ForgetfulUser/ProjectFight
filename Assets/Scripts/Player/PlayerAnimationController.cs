using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator Animator;
    public List<Sprite> sprites = new List<Sprite>();
    private Sprite jumpSprite;
    private bool lastJumpVal;
    public void StartAnimation(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                Animator.SetBool("Is Player One", true);
                break;
            case 1:
                Animator.SetBool("Is Player Two", true);
                break;
            case 2:
                Animator.SetBool("Is Player Three", true);
                break;
            case 3:
                Animator.SetBool("Is Player Four", true);
                break;
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
        //Animator.SetBool("Is Jumping", lastJumpVal);
    }
}
