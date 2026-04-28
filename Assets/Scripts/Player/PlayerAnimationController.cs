using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator Animator;
    
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

    public void UpdateAnimation(Vector2 moveDir)
    {
        Animator.SetInteger("Horizontal Movement", Mathf.RoundToInt(moveDir.x));
        Animator.SetInteger("Vertical Movement", Mathf.RoundToInt(moveDir.y));
    }
}
